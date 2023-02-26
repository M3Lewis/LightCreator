using Eto.Drawing;
using Rhino;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects;
using Eto.Forms;
using Rhino.UI;
using Grasshopper.Kernel.Types.Transforms;

namespace LightCreator.Class
{
    public class LightEditor
    {
        public bool Run;
        public List<Guid> LightGuids;
        public double LightLength;
        public double LightWidth;
        public double LightIntensity;
        public System.Drawing.Color LightColor;
        public bool IsRandomColor;
        public double RotateAngle;
        public double SpotLightAngle;

        public Vector3d vLength = Plane.WorldXY.XAxis;
        public Vector3d vWidth = Plane.WorldXY.YAxis;

        List<LightObject> lightObjects = new List<LightObject>();
        List<System.Drawing.Color> colorList = new List<System.Drawing.Color>();
        List<Point3d> grips = new List<Point3d>();

        private System.Drawing.Color GetRandomKnownColor()
        {
            var known_color = Colors[Random.Next(0, Colors.Count)];
            return System.Drawing.Color.FromKnownColor(known_color);
        }

        private System.Random m_random;
        private List<System.Drawing.KnownColor> m_colors;

        private Random Random
        {
            get
            {
                if (null == m_random)
                    m_random = new Random(Guid.NewGuid().GetHashCode());
                return m_random;
            }
        }

        private List<System.Drawing.KnownColor> Colors
        {
            get
            {
                if (null == m_colors)
                {
                    m_colors = Enum.GetValues(typeof(System.Drawing.KnownColor))
                      .Cast<System.Drawing.KnownColor>()
                      .Where(clr => !System.Drawing.Color.FromKnownColor(clr).IsSystemColor)
                      .ToList();
                }
                return m_colors;
            }
        }


        public LightEditor(bool run, List<Guid> lightGuid, double lightLength, double lightWidth, double lightIntensity, System.Drawing.Color lightColor, bool isRandomColor, double rotateAngle,double spotLightAngle)
        {
            Run = run;
            LightGuids = lightGuid;
            IsRandomColor = isRandomColor;
            LightColor = lightColor;
            RotateAngle = rotateAngle;

            if (spotLightAngle >= 90)
            {
                SpotLightAngle = 89.9;
            }
            if (spotLightAngle <= 0)
            {
                SpotLightAngle = 1;
            }
            SpotLightAngle = spotLightAngle;

            if (lightLength < 0.01)
            {
                LightLength = 0.01;
            }
            else { LightLength = lightLength; }

            if (lightWidth < 0.01)
            {
                LightWidth = 0.01;
            }
            else { LightWidth = lightWidth; }


            if (lightIntensity < 1)
                LightIntensity = 1;
            else LightIntensity = lightIntensity;
        }

        public void EditLightAttributes()
        {
            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            List<Point3d> grips = new List<Point3d>();
            if (LightGuids.Count > 0)
            {
                for (int i = 0; i < LightGuids.Count; i++)
                {
                    LightObject lo = doc.Objects.FindId(LightGuids[i]) as LightObject;

                    lightObjects.Add(lo);

                    //矩形灯控制
                    lightObjects[i].LightGeometry.Intensity = LightIntensity / 100;
                    Vector3d vLength = lightObjects[i].LightGeometry.Length;
                    vLength.Unitize();
                    lightObjects[i].LightGeometry.Length = LightLength * vLength;
                    Vector3d vWidth = lightObjects[i].LightGeometry.Width;
                    vWidth.Unitize();
                    lightObjects[i].LightGeometry.Width = LightWidth * vWidth;
                    lightObjects[i].LightGeometry.SpotAngleRadians = RhinoMath.ToRadians(SpotLightAngle);

                    //投光灯控制点
                    if (lightObjects[i].LightGeometry.IsSpotLight == true)
                    {
                        lightObjects[i].GripsOn = true;
                        List<GripObject> gpList = lightObjects[i].GetGrips().ToList();

                        foreach (var gp in gpList)
                        {
                            grips.Add(gp.CurrentLocation);
                        }

                        Point3d center = grips[0];
                        Point3d ptZ = grips[1];
                        Point3d ptLeft = grips[3];
                        Point3d ptRight = grips[2];

                        Vector3d direction = ptZ - center;
                        Plane p = new Plane(center, -direction);

                        //投光灯长度
                        double currentDirLength = direction.Length;
                        double factor = LightLength / currentDirLength;
                        lightObjects[i].LightGeometry.Direction = factor * direction;
                    }

                    //颜色控制
                    if (IsRandomColor == false)
                    {
                        lightObjects[i].LightGeometry.Diffuse = LightColor;
                    }

                    if (IsRandomColor == true)
                    {
                        if (LightGuids.Count > 0)
                        {
                            var colors = new System.Drawing.Color[LightGuids.Count];
                            colors[i] = GetRandomKnownColor();
                            lightObjects[i].LightGeometry.Diffuse = colors[i];
                        }
                    }
                    vLength = lightObjects[i].LightGeometry.Length;
                    vWidth = lightObjects[i].LightGeometry.Width;

                    lo.CommitChanges();
                }
            }
        }
        public List<string> GetRandomColorList()
        {
            List<string> colorStrList = new List<string>();
            for (int i = 0; i < lightObjects.Count; i++)
            {
                colorList.Add(lightObjects[i].LightGeometry.Diffuse);
                String colorStr = colorList[i].R + "," + colorList[i].G + "," + colorList[i].B;
                colorStrList.Add(colorStr);
            }
            return colorStrList;
        }

        Plane p1 = Plane.WorldXY;
        Plane p2 = Plane.WorldXY;
        public void RotateLightObject()
        {
            if (Run)
            {
                for (int i = 0; i < LightGuids.Count; i++)
                {
                    lightObjects[i].GripsOn = true;
                    List<GripObject> gpList = lightObjects[i].GetGrips().ToList();

                    foreach (var gp in gpList)
                    {
                        grips.Add(gp.CurrentLocation);
                    }

                    Rhino.Geometry.Light li = lightObjects[i].LightGeometry;
                    if (lightObjects[i] != null && lightObjects[i].GetGrips()!=null)
                    {
                        try
                        {
                            Point3d center = grips[0];
                            Point3d ptZ = grips[1];
                            Point3d ptX = grips[3];
                            Point3d ptY = grips[2];
                            Vector3d direction = ptZ - center;
                            double rotateRotateAngle = RhinoMath.ToRadians(RotateAngle);

                            Plane pln1 = new Plane(center, ptX - center, ptY - center);
                            Plane pln2 = new Plane(center, ptX - center, ptY - center);
                            pln2.Rotate(rotateRotateAngle, direction, center);

                            Transform xform = Transform.Rotation(rotateRotateAngle, direction, center);
                            RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
                            var geometry = lightObjects[i].Geometry.Duplicate();
                            if (null != geometry && geometry.Transform(xform))
                                doc.Objects.Transform(lightObjects[i].Id, xform, true);

                            grips.Clear();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("error!");
                        }
                        

                        
                    }
                }
            }
        }
    }
}

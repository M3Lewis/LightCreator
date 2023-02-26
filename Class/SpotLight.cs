using Eto.Drawing;
using Eto.Forms;
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

namespace LightCreator.Class
{
    public class SpotLight
    {
        public bool Run;
        public Surface BaseSrf;
        public Curve BaseCrv;
        public List<Line> BaseLine;
        public bool CountMode;
        public int LightMode;
        public int DivideCount;
        public double DivideLength;
        public double LightLength;
        public double LightWidth;
        public double LightOffsetDist;
        public double LightIntensity;
        public System.Drawing.Color LightColor;
        public double SpotLightAngle;

        //SpotLight_CreateOnCrv Overload
        public SpotLight(bool run, Curve baseCrv, int lightMode, bool countMode, int divideCount, double divideLength, double lightLength, double lightIntensity, System.Drawing.Color lightColor, double spotLightAngle)
        {
            Run = run;
            BaseCrv = baseCrv;

            LightMode = lightMode;
            CountMode = countMode;

            if (divideCount < 1)
            { DivideCount = 1; }
            else { DivideCount = divideCount; }

            if (divideLength < 0.01)
            { DivideLength = 0.01; }
            else { DivideLength = divideLength; }

            if (lightLength < 0.01)
            { LightLength = 0.01; }
            else { LightLength = lightLength; }

            if (lightIntensity < 0.1)
            { LightIntensity = 0.1; }
            else { LightIntensity = lightIntensity; }

            LightColor = lightColor;

            if (spotLightAngle < 1)
            { SpotLightAngle = 1; }
            if (spotLightAngle >= 90)
            { spotLightAngle = 89; }
            else { SpotLightAngle = spotLightAngle; }

        }

        //SpotLight_CreateAlongLine Overload
        public SpotLight(bool run, List<Line> baseLine, double spotLightAngle, double lightIntensity, System.Drawing.Color lightColor)
        {
            Run = run;
            BaseLine = baseLine;

            SpotLightAngle = spotLightAngle;

            if (lightIntensity < 0.1)
            { LightIntensity = 0.1; }
            else { LightIntensity = lightIntensity; }

            LightColor = lightColor;
        }

        //SpotLight_CreateOnCrv_Value
        double[] DivideParam = null;
        List<Curve> SplitedCrvs = new List<Curve>();
        List<Plane> PlnList = new List<Plane>();
        List<double> ParamList = new List<double>();
        List<Rhino.Geometry.Light> lights = new List<Rhino.Geometry.Light>();


        public List<Curve> CreateOnCrv_DivideCurve()
        {
            try
            {
                if (CountMode == true)
                {
                    DivideParam = BaseCrv.DivideByCount(DivideCount, true);
                }
                else
                {
                    DivideParam = BaseCrv.DivideByLength(DivideLength, false);
                }

                SplitedCrvs.AddRange(BaseCrv.Split(DivideParam));
                ParamList.AddRange(DivideParam.ToList());

            }
            catch (Exception)
            {
                Console.WriteLine("Whoa!");
            }
            return SplitedCrvs;
        }
        public List<Point3d> CreateOnCrv_GetDividePoint()
        {
            List<Point3d> dividePts = new List<Point3d>();
            for (int i = 0; i < ParamList.Count; i++)
            {
                dividePts.Add(BaseCrv.PointAt(ParamList[i]));
            }
            return dividePts;
        }
        public List<Plane> CreateOnCrv_GetPlane()
        {
            for (int i = 0; i < ParamList.Count; i++)
            {
                Plane tempPln;
                BaseCrv.FrameAt(ParamList[i], out tempPln);

                //旋转平面
                if (Vector3d.Multiply(tempPln.Normal, Plane.WorldXY.ZAxis) == 1)
                {
                    tempPln.Flip();
                    tempPln.Rotate(RhinoMath.ToRadians(SpotLightAngle + 90), tempPln.ZAxis, tempPln.Origin);
                }
                else
                {
                    tempPln.Rotate(RhinoMath.ToRadians(SpotLightAngle), tempPln.ZAxis, tempPln.Origin);
                }
                PlnList.Add(tempPln);
            }
            return PlnList;
        }
        public void CreateOnCrv_DefineSpotLightGeometry()
        {
            for (int i = 0; i < ParamList.Count; i++)
            {
                Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                lights.Add(li);
                lights[i].LightStyle = LightStyle.WorldSpot;
                lights[i].Intensity = LightIntensity / 100;
                lights[i].Diffuse = LightColor;
                lights[i].Direction = PlnList[i].ZAxis * LightLength;
                lights[i].Location = PlnList[i].Origin;
                lights[i].SpotAngleRadians = RhinoMath.ToRadians(SpotLightAngle);
            }
        }

        public void CreateAlongLine_DefineSpotLightGeometry()
        {
            for (int i = 0; i < BaseLine.Count; i++)
            {
                Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                lights.Add(li);
                lights[i].LightStyle = LightStyle.WorldSpot;
                lights[i].Location = BaseLine[i].From;
                lights[i].Direction = BaseLine[i].To - BaseLine[i].From;
                lights[i].SpotAngleRadians = SpotLightAngle * Math.PI / 180;
                lights[i].Intensity = LightIntensity / 100;
                lights[i].Diffuse = LightColor;
            }
        }
        public void CreateSpotLight()
        {
            if (Run)
            {
                Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
                foreach (var light in lights)
                {
                    doc.Lights.Add(light);
                }
            }
        }


        public List<Rhino.Geometry.Mesh> CreateOnCrv_GetSpotLightPreviewMesh()
        {
            List<Cone> coneList = new List<Cone>();
            List<Rhino.Geometry.Mesh> spotLightPreviewMesh = new List<Rhino.Geometry.Mesh>();
            for (int i = 0; i < ParamList.Count; i++)
            {
                double d = LightLength * Math.Tan(RhinoMath.ToRadians(SpotLightAngle));
                coneList.Add(new Cone(PlnList[i], LightLength, d));
                Brep b = coneList[i].ToBrep(false);
                spotLightPreviewMesh.AddRange(Rhino.Geometry.Mesh.CreateFromBrep(b, MeshingParameters.Minimal));
            }
            return spotLightPreviewMesh;
        }

        public List<Rhino.Geometry.Mesh> CreateAlongLine_GetSpotLightPreviewMesh()
        {
            List<Cone> coneList = new List<Cone>();
            List<Rhino.Geometry.Mesh> spotLightPreviewMesh = new List<Rhino.Geometry.Mesh>();
            for (int i = 0; i < BaseLine.Count; i++)
            {
                double d = BaseLine[i].Length * Math.Tan(RhinoMath.ToRadians(SpotLightAngle));
                Plane p;
                BaseLine[i].ToNurbsCurve().DuplicateCurve().FrameAt(0, out p);
                p.Rotate(RhinoMath.ToRadians(90), p.YAxis, p.Origin);
                coneList.Add(new Cone(p, BaseLine[i].Length, d));
                Brep b = coneList[i].ToBrep(false);


                spotLightPreviewMesh.AddRange(Rhino.Geometry.Mesh.CreateFromBrep(b, MeshingParameters.Minimal));
            }
            return spotLightPreviewMesh;
        }
    }
}


using Rhino;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightCreator.Class
{
    public class PointLight
    {
        public bool Run;
        public List<Point3d> Location;
        public Curve BaseCrv;
        public bool CountMode;
        public int LightMode;
        public int DivideCount;
        public double DivideLength;
        public double LightLength;
        public double LightWidth;
        public double LightIntensity;
        public List<Color> LightColor;
        public double Angle;


        public double LightRadius;


        public PointLight(bool run, List<Point3d> location, double lightIntensity, List<Color> lightColor, double lightRadius)
        {
            Run = run;

            if (location.Count == 0)
            {
                Location.Add(Plane.WorldXY.Origin);
            }
            else
            {
                Location = location;
            }

            LightRadius = lightRadius;

            if (lightIntensity < 0.1)
                lightIntensity = 0.1;
            else LightIntensity = lightIntensity;

            LightColor = lightColor;
        }
        //CreateOnCrv Overload
        public PointLight(bool run, Curve baseCrv, int lightMode, bool countMode, double lightRadius, int divideCount, double divideLength,
            double lightLength, double lightWidth, double lightIntensity, List<Color> lightColor)
        {
            Run = run;
            BaseCrv = baseCrv;

            LightMode = lightMode;
            CountMode = countMode;


            LightRadius = lightRadius;

            DivideCount = divideCount;
            if (divideLength < 0.01)
            { DivideLength = 0.01; }
            else { DivideLength = divideLength; }

            if (lightLength < 0.01)
            { LightLength = 0.01; }
            else { LightLength = lightLength; }

            if (lightWidth < 0.01)
            { LightWidth = 0.01; }
            else { LightWidth = lightWidth; }

            if (lightIntensity < 0.1)
            { LightIntensity = 0.1; }
            else { LightIntensity = lightIntensity; }

            LightColor = lightColor;
        }

        double[] DivideParam = null;
        List<Curve> SplitedCrvs = new List<Curve>();
        List<Plane> PlnList = new List<Plane>();
        public List<double> ParamList = new List<double>();
        List<Rhino.Geometry.Light> lights = new List<Rhino.Geometry.Light>();

        public void Default_DefinePointLightGeometry()
        {
            if (Run)
            {
                for (int i = 0; i < Location.Count; i++)
                {
                    Rhino.Geometry.Light li = new Rhino.Geometry.Light();


                    li.LightStyle = LightStyle.WorldPoint;
                    li.Location = Location[i];
                    li.Intensity = LightIntensity / 100;
                    if (LightColor.Count > 1)
                    {
                        li.Diffuse = LightColor[i];
                    }
                    else
                    {
                        li.Diffuse = LightColor[0];
                    }
                    lights.Add(li);
                }
            }
        }

        public List<Curve> CreateOnCrv_DivideCurve()
        {

            if (CountMode == true)
            {
                DivideParam = BaseCrv.DivideByCount(DivideCount, true);
            }
            else
            {
                DivideParam = BaseCrv.DivideByLength(DivideLength, true);
            }

            SplitedCrvs.AddRange(BaseCrv.Split(DivideParam));
            ParamList.AddRange(DivideParam.ToList());

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
                PlnList.Add(tempPln);
                Vector3d fineTunedYAxis = Vector3d.CrossProduct(tempPln.XAxis, Plane.WorldXY.ZAxis);
                tempPln = new Plane(tempPln.Origin, tempPln.XAxis, fineTunedYAxis);
            }
            return PlnList;
        }

        
        public void CreateOnCrv_DefinePointLightGeometry()
        {
            for (int i = 0; i < ParamList.Count; i++)
            {
                Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                lights.Add(li);
                lights[i].LightStyle = LightStyle.WorldPoint;
                lights[i].Intensity = LightIntensity / 100;
                if (LightColor.Count > 1)
                {
                    lights[i].Diffuse = LightColor[i];
                }
                else
                {
                    lights[i].Diffuse = LightColor[0];
                }
                lights[i].Location = PlnList[i].Origin;
            }
        }

        public List<Rhino.Geometry.Mesh> CreateOnCrv_GetPointLightPreviewMesh()
        {
            List<Rhino.Geometry.Mesh> lightPreviewObjectMesh = new List<Rhino.Geometry.Mesh>();

            for (int i = 0; i < ParamList.Count; i++)
            {
                Sphere sphere = new Sphere(PlnList[i].Origin, LightRadius);
                Brep b = sphere.ToBrep();
                lightPreviewObjectMesh.AddRange(Rhino.Geometry.Mesh.CreateFromBrep(b, MeshingParameters.Minimal));
            }
            return lightPreviewObjectMesh;
        }
        public List<Rhino.Geometry.Mesh> Default_GetPointLightPreviewMesh()
        {
            List<Rhino.Geometry.Mesh> lightPreviewObjectMesh = new List<Rhino.Geometry.Mesh>();

            for (int i = 0; i < Location.Count; i++)
            {
                Sphere sphere = new Sphere(Location[i], LightRadius);
                Brep b = sphere.ToBrep();
                lightPreviewObjectMesh.AddRange(Rhino.Geometry.Mesh.CreateFromBrep(b, MeshingParameters.Minimal));
            }
            return lightPreviewObjectMesh;
        }

        public void CreatePointLight()
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            foreach (var light in lights)
            {
                doc.Lights.Add(light);
            }
        }
    }
}

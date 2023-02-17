using Eto.Drawing;
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
        public double LightIntensity;
        public System.Drawing.Color LightColor;
        public PointLight(bool run, List<Point3d> location, double lightIntensity, System.Drawing.Color lightColor)
        {
            Run = run;
            Location = location;

            if (lightIntensity < 0.1)
                lightIntensity = 0.1;
            else LightIntensity = lightIntensity;

            LightColor = lightColor;
        }

        public void CreatePointLight()
        {
            List<Rhino.Geometry.Light> pointLights = new List<Rhino.Geometry.Light>();
            if (Run)
            {
                for (int i = 0; i < Location.Count; i++)
                {
                    Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                    pointLights.Add(li);
                }

                for (int i = 0; i < Location.Count; i++)
                {
                    pointLights[i].LightStyle = LightStyle.WorldPoint;
                    pointLights[i].Location = Location[i];
                    pointLights[i].Intensity = LightIntensity / 100;
                    pointLights[i].Diffuse = LightColor;
                }

                RhinoDoc doc = RhinoDoc.ActiveDoc;
                foreach (var light in pointLights)
                {
                    doc.Lights.Add(light);
                }
            }
        }
    }
}

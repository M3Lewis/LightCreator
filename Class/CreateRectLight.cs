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

namespace LightCreator.Class
{
    public class RectLight
    {
        public bool Run;
        public Surface BaseSrf;
        public Curve BaseCrv;
        public double DivideLength;
        public double LightWidth;
        public double LightOffsetDist;
        public double LightIntensity;
        public System.Drawing.Color LightColor;

        public double LightPreviewObjectLength;
        public double LightPreviewObjectWidth;

        private double[] DivideParam;
        private List<Curve> SplitedCrvs = null;
        private List<Plane> PlaneList = null;


        //CreateOnSrf Overload
        public RectLight(bool run, Surface baseSrf, Curve baseCrv, double divideLength, double lightWidth, double lightOffsetDist, double lightIntensity, System.Drawing.Color lightColor)
        {
            Run = run;
            LightColor = lightColor;

            BaseSrf = baseSrf;
            BaseCrv = baseCrv;

            LightOffsetDist = lightOffsetDist;

            if (divideLength < 0.1)
                DivideLength = 0.1;
            else DivideLength = divideLength;

            if (lightWidth < 0.1)
                LightWidth = 0.1;
            else LightWidth = lightWidth;

            if (lightIntensity < 0.1)
                LightIntensity = 0.1;
            else LightIntensity = lightIntensity;
        }

        //CreateFitSrf Overload
        public RectLight(bool run, Surface baseSrf, double lightIntensity, System.Drawing.Color lightColor)
        {
            Run = run;
            LightColor = lightColor;
            BaseSrf = baseSrf;

            if (lightIntensity < 0.1)
                LightIntensity = 0.1;
            else LightIntensity = lightIntensity;
        }

        public List<Curve> CreateOnSrf_DivideCurve()
        {
            //分割曲线
            //两种情况
            //1.曲线长度大于divLen -> 分割曲线
            //2.曲线长度小于divLen -> 直接采用曲线本身
            List<Curve> splitedCrvs = new List<Curve>();
            Curve[] crvsArray = BaseCrv.DuplicateSegments();

            for (int i = 0; i < crvsArray.Length; i++)
            {
                double inputCrvLen = crvsArray[i].GetLength();
                if (inputCrvLen >= DivideLength)
                {
                    DivideParam = crvsArray[i].DivideByLength(DivideLength, false);
                    Curve[] tempCrvs = crvsArray[i].Split(DivideParam);
                    splitedCrvs.AddRange(tempCrvs);
                }
                else
                {
                    Curve tempShortCrv = crvsArray[i];
                    splitedCrvs.Add(tempShortCrv);
                }
            }
            SplitedCrvs = splitedCrvs;
            return splitedCrvs;
        }
        public List<Point3d> CreateOnSrf_GetDividePoint()
        {
            List<Point3d> startPts = new List<Point3d>();
            List<double> planeParam = new List<double>();

            for (int i = 0; i < SplitedCrvs.Count; i++)
            {
                double t;
                SplitedCrvs[i].ClosestPoint(SplitedCrvs[i].PointAtStart, out t);
                planeParam.Add(t);
            }

            //求分割点
            List<Point3d> dividePts = new List<Point3d>();

            for (int i = 0; i < planeParam.Count; i++)
            {
                dividePts.Add(BaseCrv.PointAt(planeParam[i]));
            }
            return dividePts;
        }

        public List<Plane> CreateOnSrf_AdjustPlane()
        {
            //确定平面
            List<Plane> planeList = new List<Plane>();
            List<double> PlaneParam = new List<double>();

            double U;
            double V;

            List<double> UList = new List<double>();
            List<double> VList = new List<double>();
            List<Vector3d> normalList = new List<Vector3d>();

            List<object> obj = new List<object>();

            for (int i = 0; i < SplitedCrvs.Count; i++)
            {
                Plane tempPlane;
                double startParam;

                Point3d startPoint = SplitedCrvs[i].PointAtStart;
                SplitedCrvs[i].ClosestPoint(startPoint, out startParam);

                SplitedCrvs[i].FrameAt(startParam, out tempPlane);
                BaseSrf.ClosestPoint(startPoint, out U, out V);
                PlaneParam.Add(startParam);

                UList.Add(U);
                VList.Add(V);
                Vector3d normal = BaseSrf.NormalAt(UList[i], VList[i]);
                normalList.Add(normal);

                //调用Adjust Plane
                Rhino.NodeInCode.ComponentFunctionInfo PlaneInfo = Rhino.NodeInCode.Components.FindComponent("AdjustPlane");
                System.Delegate outAdjustedPlane = PlaneInfo.Delegate;
                IList<object> resultPlane = (IList<object>)outAdjustedPlane.DynamicInvoke(tempPlane, normalList[i]);
                IList<object> resultPlaneList = resultPlane;
                obj.Add(resultPlane[0]);

                Plane pp = (Rhino.Geometry.Plane)resultPlaneList[0];
                planeList.Add(pp);

            }
            PlaneList = planeList;
            return planeList;
        }


        List<Rhino.Geometry.Light> lightList = new List<Rhino.Geometry.Light>();
        public void CreateOnSrf_DefineLightGeometry()
        {
            //添加灯光到List
            for (int i = 0; i < SplitedCrvs.Count; i++)
            {
                Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                lightList.Add(li);
            }

            foreach (Rhino.Geometry.Light li in lightList)
            {
                li.LightStyle = LightStyle.WorldRectangular;
            }

            List<Line> ln1List = new List<Line>();
            List<Line> ln2List = new List<Line>();

            for (int i = 0; i < SplitedCrvs.Count; i++)
            {
                lightList[i].Location = SplitedCrvs[i].PointAtStart;

                //定义矩形灯照射方向
                if (Vector3d.Multiply(PlaneList[i].Normal, Plane.WorldXY.ZAxis) == -1)
                {
                    lightList[i].Direction = -PlaneList[i].Normal;
                }

                else
                {
                    lightList[i].Direction = PlaneList[i].Normal;
                }

                //定义矩形灯长度宽度
                Vector3d v1 = SplitedCrvs[i].PointAtEnd - SplitedCrvs[i].PointAtStart;
                Vector3d v2 = PlaneList[i].XAxis;
                double angle = Vector3d.VectorAngle(v1, v2);
                double lightLen = SplitedCrvs[i].PointAtEnd.DistanceTo(SplitedCrvs[i].PointAtStart);

                lightList[i].Width = LightWidth * PlaneList[i].YAxis;
                lightList[i].Length = v2 * lightLen;

                lightList[i].Rotate(-angle, PlaneList[i].YAxis, PlaneList[i].Origin);

                ln1List.Add(new Line(PlaneList[i].Origin, lightList[i].Length));
                ln2List.Add(new Line(PlaneList[i].Origin, v1));

                NurbsCurve nc1 = ln1List[i].ToNurbsCurve();
                NurbsCurve nc2 = ln2List[i].ToNurbsCurve();

                Point3d tempPt1 = nc1.PointAtEnd;
                Point3d tempPt2 = nc2.PointAtEnd;

                List<Point3d> tempPtList = new List<Point3d>();
                tempPtList.Add(tempPt1);
                tempPtList.Add(tempPt2);
                tempPtList.Add(PlaneList[i].Origin);

                Plane finalPlane;

                Plane.FitPlaneToPoints(tempPtList, out finalPlane);
                double angle2 = Vector3d.VectorAngle(lightList[i].Length, v1, finalPlane);
                double finalAngle = -angle2;

                if (finalAngle > 180 && finalAngle < 360)
                {
                    lightList[i].Rotate(-angle2, finalPlane.ZAxis, PlaneList[i].Origin);
                }
                else
                {
                    lightList[i].Rotate(angle2, finalPlane.ZAxis, PlaneList[i].Origin);
                }

                Vector3d vw = -0.5 * lightList[i].Width;
                Transform tw = Transform.Translation(vw);
                lightList[i].Transform(tw);

                Vector3d vo = LightOffsetDist * lightList[i].Direction;
                Transform to = Transform.Translation(vo);
                lightList[i].Transform(to);
            }

            //定义矩形灯亮度
            for (int i = 0; i < PlaneList.Count; i++)
            {
                lightList[i].Intensity = LightIntensity / 100;
            }

            //定义矩形灯颜色
            for (int i = 0; i < PlaneList.Count; i++)
            {
                lightList[i].Diffuse = LightColor;
            }

        }

        public List<Rhino.Geometry.Mesh> GetRectLightPreviewMesh()
        {
            List<Rhino.Geometry.Mesh> lightPreviewObjectMesh = new List<Rhino.Geometry.Mesh>();
            for (int i = 0; i < lightList.Count; i++)
            {
                Plane boxPlane = new Plane(lightList[i].Location, lightList[i].Length, lightList[i].Width);
                Rectangle3d rect = new Rectangle3d(boxPlane, lightList[i].Length.Length, lightList[i].Width.Length);
                Curve c = (Curve)rect.ToNurbsCurve();
                Extrusion ext = Extrusion.Create(c, 0.1, true);
                Brep b = ext.ToBrep();
                List<Rhino.Geometry.Mesh> meshList = new List<Rhino.Geometry.Mesh>();

                MeshingParameters mParam = new MeshingParameters(0);
                meshList.AddRange(Rhino.Geometry.Mesh.CreateFromBrep(b, mParam));

                Rhino.Geometry.Mesh tempMesh = new Rhino.Geometry.Mesh();
                tempMesh.Append(meshList);

                lightPreviewObjectMesh.Add(tempMesh);
            }
            return lightPreviewObjectMesh;
        }

        public void CreateFitPlaneSrf_DefineGeometry()
        {
            //添加灯光到List

            Rhino.Geometry.Light light = new Rhino.Geometry.Light();
            lightList.Add(light);

            foreach (Rhino.Geometry.Light li in lightList)
            {
                li.LightStyle = LightStyle.WorldRectangular;
                //求面边缘
                Brep b = BaseSrf.ToBrep();
                Curve[] edges = b.DuplicateEdgeCurves(false);

                //求角点
                Rhino.NodeInCode.ComponentFunctionInfo cornerInfo = Rhino.NodeInCode.Components.FindComponent("Discontinuity");
                System.Delegate outMyCorner = cornerInfo.Delegate;
                IList<object> resultCorner = (IList<object>)outMyCorner.DynamicInvoke(edges, 1);

                Point3d resultPts1 = ((Point3d)((IList)resultCorner[0])[0]);
                Point3d resultPts2 = ((Point3d)((IList)resultCorner[0])[1]);
                Point3d resultPts3 = ((Point3d)((IList)resultCorner[0])[2]);
                Point3d resultPts4 = ((Point3d)((IList)resultCorner[0])[3]);

                //定位
                double u;
                double v;

                BaseSrf.ClosestPoint(resultPts2, out u, out v);

                Point3d cp = BaseSrf.PointAt(u, v);
                Vector3d vDir = BaseSrf.NormalAt(u, v);
                Plane pn = new Plane(cp, resultPts1, resultPts4);

                //赋予长宽
                double sWidth;
                double sHeight;
                BaseSrf.GetSurfaceSize(out sWidth, out sHeight);
                li.Location = pn.Origin;
                li.Direction = -vDir;
                li.Length = pn.XAxis * sWidth;
                li.Width = pn.YAxis * sHeight;
                li.Diffuse = LightColor;
            }
        }

        public void CreateRectLight()
        {
            if (Run)
            {
                //生成灯光
                RhinoDoc doc = RhinoDoc.ActiveDoc;
                foreach (Rhino.Geometry.Light li in lightList)
                {
                    doc.Lights.Add(li);
                }
            }
        }
    }
}

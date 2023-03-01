
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
    public class RectLight
    {
        public bool Run;
        public Surface BaseSrf;
        public List<Surface> BaseSrfList;
        public Curve BaseCrv;
        public int LightMode;
        public bool CountMode;
        public int DivideCount;
        public double DivideLength;
        public double LightLength;
        public double LightWidth;
        public double LightOffsetDist;
        public double LightIntensity;
        public List<Color> LightColor;
        public double RectLightAngle;

        //public double LightPreviewObjectLength;
        //public double LightPreviewObjectWidth;


        //CreateOnSrf Overload
        public RectLight(bool run, Surface baseSrf, Curve baseCrv, double lightLength, double lightWidth, double lightOffsetDist, double lightIntensity, List<Color> lightColor)
        {
            Run = run;
            LightColor = lightColor;

            BaseSrf = baseSrf;
            BaseCrv = baseCrv;

            LightOffsetDist = lightOffsetDist;

            if (lightLength < 0.01)
            {
                LightLength = 0.01;
            }
            else
            {
                LightLength = lightLength;
            }


            if (lightWidth < 0.01)
            {
                LightWidth = 0.01;
            }
            else
            {
                LightWidth = lightWidth;
            }

            if (lightIntensity < 0.1)
                LightIntensity = 0.1;
            else LightIntensity = lightIntensity;


        }

        //CreateFitSrf Overload
        public RectLight(bool run, List<Surface> baseSrfList, double lightIntensity, List<Color> lightColor)
        {
            Run = run;
            LightColor = lightColor;
            BaseSrfList = baseSrfList;

            if (lightIntensity < 0.1)
                LightIntensity = 0.1;
            else LightIntensity = lightIntensity;
        }

        //CreateOnCrv Overload
        public RectLight(bool run, Curve baseCrv, int lightMode, bool countMode, int divideCount, double divideLength, double lightLength, double lightWidth, double lightIntensity, List<Color> lightColor, double rectLightAngle)
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

            if (lightWidth < 0.01)
            { LightWidth = 0.01; }
            else { LightWidth = lightWidth; }

            if (lightIntensity < 0.1)
            { LightIntensity = 0.1; }
            else { LightIntensity = lightIntensity; }

            LightColor = lightColor;

            RectLightAngle = rectLightAngle;
        }
        //CreateOnCrv_Value
        double[] DivideParam = null;
        List<Curve> SplitedCrvs = new List<Curve>();
        public List<Plane> PlnList = new List<Plane>();
        List<double> ParamList = new List<double>();
        List<Rhino.Geometry.Light> lights = new List<Rhino.Geometry.Light>();

        //CreateFitPlaneSrf_Value
        List<double> SrfWidth = new List<double>();
        List<double> SrfHeight = new List<double>();
        List<Plane> SrfPln = new List<Plane>(); //平面曲面角点Plane


        public List<Curve> CreateOnSrfAlongCrv_DivideCurve()
        {
            //分割曲线
            //两种情况
            //1.曲线长度大于divLen -> 分割曲线
            //2.曲线长度小于divLen -> 直接采用曲线本身
            List<Curve> splitedCrvs = new List<Curve>();

            if (BaseCrv != null)
            {
                Curve[] crvsArray = BaseCrv.DuplicateSegments();
                for (int i = 0; i < crvsArray.Length; i++)
                {
                    double inputCrvLen = crvsArray[i].GetLength();
                    if (inputCrvLen >= LightLength)
                    {
                        DivideParam = crvsArray[i].DivideByLength(LightLength, false);
                        Curve[] tempCrvs = crvsArray[i].Split(DivideParam);
                        splitedCrvs.AddRange(tempCrvs);
                    }
                    else
                    {
                        Curve tempShortCrv = crvsArray[i];
                        splitedCrvs.Add(tempShortCrv);
                    }
                }
            }
            SplitedCrvs = splitedCrvs;
            return splitedCrvs;
        }
        public List<Point3d> CreateOnSrfAlongCrv_GetDividePoint()
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
        public List<Plane> CreateOnSrfAlongCrv_AdjustPlane()
        {
            //确定平面

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
                ParamList.Add(startParam);

                UList.Add(U);
                VList.Add(V);
                Vector3d normal = BaseSrf.NormalAt(UList[i], VList[i]);
                normalList.Add(normal);

                //调用Adjust Plane
                Rhino.NodeInCode.ComponentFunctionInfo PlaneInfo = Rhino.NodeInCode.Components.FindComponent("AdjustPlane");
                System.Delegate outAdjustedPlane = PlaneInfo.Delegate;
                IList<object> resultPlane = (IList<object>)outAdjustedPlane.DynamicInvoke(tempPlane, normalList[i]);
                IList<object> resultPlnList = resultPlane;
                obj.Add(resultPlane[0]);

                Plane pp = (Rhino.Geometry.Plane)resultPlnList[0];
                PlnList.Add(pp);
            }
            return PlnList;
        }
        public void CreateOnSrfAlongCrv_DefineLightGeometry()
        {
            //添加灯光到List
            for (int i = 0; i < SplitedCrvs.Count; i++)
            {
                Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                lights.Add(li);
            }

            foreach (Rhino.Geometry.Light li in lights)
            {
                li.LightStyle = LightStyle.WorldRectangular;
            }

            List<Line> ln1List = new List<Line>();
            List<Line> ln2List = new List<Line>();

            for (int i = 0; i < SplitedCrvs.Count; i++)
            {
                lights[i].Location = SplitedCrvs[i].PointAtStart;

                //定义矩形灯照射方向
                if (Vector3d.Multiply(PlnList[i].Normal, Plane.WorldXY.ZAxis) == -1)
                {
                    lights[i].Direction = -PlnList[i].Normal;
                }

                else
                {
                    lights[i].Direction = PlnList[i].Normal;
                }

                //定义矩形灯长度宽度
                Vector3d v1 = SplitedCrvs[i].PointAtEnd - SplitedCrvs[i].PointAtStart;
                Vector3d v2 = PlnList[i].XAxis;
                double angle = Vector3d.VectorAngle(v1, v2);
                double lightLen = SplitedCrvs[i].PointAtEnd.DistanceTo(SplitedCrvs[i].PointAtStart);

                lights[i].Width = LightWidth * PlnList[i].YAxis;
                lights[i].Length = v2 * lightLen;

                lights[i].Rotate(-angle, PlnList[i].YAxis, PlnList[i].Origin);

                ln1List.Add(new Line(PlnList[i].Origin, lights[i].Length));
                ln2List.Add(new Line(PlnList[i].Origin, v1));

                NurbsCurve nc1 = ln1List[i].ToNurbsCurve();
                NurbsCurve nc2 = ln2List[i].ToNurbsCurve();

                Point3d tempPt1 = nc1.PointAtEnd;
                Point3d tempPt2 = nc2.PointAtEnd;

                List<Point3d> tempPtList = new List<Point3d>();
                tempPtList.Add(tempPt1);
                tempPtList.Add(tempPt2);
                tempPtList.Add(PlnList[i].Origin);

                Plane finalPlane;

                Plane.FitPlaneToPoints(tempPtList, out finalPlane);
                double angle2 = Vector3d.VectorAngle(lights[i].Length, v1, finalPlane);
                double finalAngle = -angle2;

                if (finalAngle > 180 && finalAngle < 360)
                {
                    lights[i].Rotate(-angle2, finalPlane.ZAxis, PlnList[i].Origin);
                }
                else
                {
                    lights[i].Rotate(angle2, finalPlane.ZAxis, PlnList[i].Origin);
                }

                //对齐到线中
                Vector3d vw = -0.5 * lights[i].Width;
                Transform tw = Transform.Translation(vw);
                lights[i].Transform(tw);

                //面法线偏移
                Vector3d vo = LightOffsetDist * lights[i].Direction;
                Transform to = Transform.Translation(vo);
                lights[i].Transform(to);
            }

            //定义矩形灯亮度
            for (int i = 0; i < PlnList.Count; i++)
            {
                lights[i].Intensity = LightIntensity / 100;
            }

            //定义矩形灯颜色
            for (int i = 0; i < PlnList.Count; i++)
            {
                if (LightColor.Count > 1 || LightColor.Count == PlnList.Count || CountMode == true)
                {
                    lights[i].Diffuse = LightColor[i];
                }
                else
                {
                    lights[i].Diffuse = LightColor[0];
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
                Vector3d fineTunedYAxis = Vector3d.CrossProduct(tempPln.XAxis, Plane.WorldXY.ZAxis);
                tempPln = new Plane(tempPln.Origin, tempPln.XAxis, fineTunedYAxis);
                tempPln.Rotate(RhinoMath.ToRadians(RectLightAngle), tempPln.ZAxis);
                PlnList.Add(tempPln);
            }
            return PlnList;
        }

        public void CreateOnCrv_DefineRectLightGeometry()
        {
            //生成灯光
            for (int i = 0; i < ParamList.Count; i++)
            {
                Rhino.Geometry.Light li = new Rhino.Geometry.Light();
                lights.Add(li);
                lights[i].LightStyle = LightStyle.WorldRectangular;
                lights[i].Intensity = LightIntensity / 100;
                if (LightColor.Count > 1)
                {
                    lights[i].Diffuse = LightColor[i];
                }
                else
                {
                    lights[i].Diffuse = LightColor[0];
                }
                lights[i].Direction = PlnList[i].ZAxis;
                lights[i].Location = PlnList[i].Origin;
                lights[i].Length = PlnList[i].XAxis * LightLength;
                lights[i].Width = PlnList[i].YAxis * LightWidth;

                //对齐到线中
                Vector3d vw = -0.5 * lights[i].Width;
                Transform tw = Transform.Translation(vw);
                lights[i].Transform(tw);
            }
        }


        public List<Rectangle3d> CreateOnCrv_GetRectLightPreviewCrvs()
        {
            List<Rectangle3d> RectLightPreviewCrvs = new List<Rectangle3d>();

            for (int i = 0; i < ParamList.Count; i++)
            {
                RectLightPreviewCrvs.Add(new Rectangle3d(new Plane(lights[i].Location, lights[i].Length, lights[i].Width), LightLength, LightWidth));
            }
            return RectLightPreviewCrvs;
        }
        public List<Line> CreateOnCrv_GetDirectionLine()
        {
            List<Line> dirList = new List<Line>();
            for (int i = 0; i < PlnList.Count; i++)
            {
                Vector3d vZ = PlnList[i].ZAxis;

                Transform xForm1 = Transform.Translation(vZ*((LightLength+LightWidth)/1.2));
                Transform xForm2 = Transform.Translation(0.5 * PlnList[i].XAxis*LightLength);
                Point3d dirPtEnd = PlnList[i].Origin;
                Point3d dirPtStart = PlnList[i].Origin;
                dirPtEnd.Transform(xForm1);
                dirPtEnd.Transform(xForm2);
                dirPtStart.Transform(xForm2);
                Line dirLine = new Line(dirPtStart, dirPtEnd);
                dirList.Add(dirLine);
            }
            return dirList;
        }

        public List<Rectangle3d> CreateOnSrfAlongCrv_GetRectLightPreviewCrvs()
        {
            List<Rectangle3d> RectLightPreviewCrvs = new List<Rectangle3d>();
            for (int i = 0; i < lights.Count; i++)
            {
                Plane boxPlane = new Plane(lights[i].Location, lights[i].Length, lights[i].Width);
                RectLightPreviewCrvs.Add(new Rectangle3d(boxPlane, lights[i].Length.Length, lights[i].Width.Length));
            }
            return RectLightPreviewCrvs;
        }


        public List<Rectangle3d> CreateFitPlaneSrf_GetRectLightPreviewCrvs()
        {
            List<Rectangle3d> RectLightPreviewCrvs = new List<Rectangle3d>();
            for (int i = 0; i < BaseSrfList.Count; i++)
            {
                RectLightPreviewCrvs.Add(new Rectangle3d(SrfPln[i], SrfWidth[i], SrfHeight[i]));
            }
            return RectLightPreviewCrvs;
        }

        public void CreateFitPlaneSrf_DefineGeometry()
        {
            for (int i = 0; i < BaseSrfList.Count; i++)
            {
                //添加灯光到List

                Rhino.Geometry.Light light = new Rhino.Geometry.Light();
                lights.Add(light);


                lights[i].LightStyle = LightStyle.WorldRectangular;
                //求面边缘
                Brep b = BaseSrfList[i].ToBrep();
                Curve[] edges = b.DuplicateEdgeCurves(false);

                //求角点
                Rhino.NodeInCode.ComponentFunctionInfo cornerInfo = Rhino.NodeInCode.Components.FindComponent("Discontinuity");
                System.Delegate outMyCorner = cornerInfo.Delegate;
                IList<object> resultCorner = (IList<object>)outMyCorner.DynamicInvoke(edges, 1);

                Point3d resultPts1 = ((Point3d)((IList)resultCorner[0])[0]);
                Point3d resultPts2 = ((Point3d)((IList)resultCorner[0])[1]);
                Point3d resultPts4 = ((Point3d)((IList)resultCorner[0])[3]);

                //定位
                double u;
                double v;

                BaseSrfList[i].ClosestPoint(resultPts2, out u, out v);

                Point3d cp = BaseSrfList[i].PointAt(u, v);
                Vector3d vDir = BaseSrfList[i].NormalAt(u, v);
                SrfPln.Add(new Plane(cp, resultPts1, resultPts4));

                //赋予长宽
                double tempSrfWidth;
                double tempSrfHeight;
                BaseSrfList[i].GetSurfaceSize(out tempSrfWidth, out tempSrfHeight);
                SrfWidth.Add(tempSrfWidth);
                SrfHeight.Add(tempSrfHeight);
                lights[i].Location = SrfPln[i].Origin;
                lights[i].Direction = -vDir;
                lights[i].Length = SrfPln[i].XAxis * SrfWidth[i];
                lights[i].Width = SrfPln[i].YAxis * SrfHeight[i];
                if (LightColor.Count > 1)
                {
                    lights[i].Diffuse = LightColor[i];
                }
                else
                {
                    lights[i].Diffuse = LightColor[0];
                }
                lights[i].Intensity = LightIntensity / 100;
            }


        }

        public void CreateRectLight()
        {
            if (Run)
            {
                //生成灯光
                RhinoDoc doc = RhinoDoc.ActiveDoc;
                foreach (Rhino.Geometry.Light li in lights)
                {
                    doc.Lights.Add(li);
                }
            }
        }
    }
}

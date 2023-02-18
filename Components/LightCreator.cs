using Grasshopper;
using Grasshopper.Kernel;
using LightCreator.Class;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Types;
using Rhino;
using Grasshopper.Kernel.Parameters;

namespace LightCreator
{
    public class RecLightOnSrfComponent : GH_Component
    {
        public RecLightOnSrfComponent()
          : base("RectLightOnSrf", "RectLightOnSrf",
            "Create Rectangular Light from Base Surface and Base Curves.Light Direction is equal to Surface Normal Direction.| 在曲面上根据曲线，生成对应曲面法线方向的矩形灯",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | 开关", GH_ParamAccess.item, false);
            pManager.AddSurfaceParameter("srf", "srf", "Base Surface | 基准面", GH_ParamAccess.item);
            pManager.AddCurveParameter("crv", "crv", "Base Curves | 基准线", GH_ParamAccess.item);
            pManager.AddNumberParameter("lightLength", "lightLength", "Length of Rectangular Light | 矩形灯长度", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("lightWidth", "lightWidth", "Width of Rectangular Light | 矩形灯宽度", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("offsetDist", "offsetDist", "Distance From Light to Surface |灯光到基准面偏移距离", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | 灯光亮度", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | 灯光颜色", GH_ParamAccess.item, Color.Red);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddCurveParameter("Segments", "Segments", "Splited Base Curves | 按灯光长度分段的基准曲线", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points | 分段点", GH_ParamAccess.list);
            pManager.AddMeshParameter("PreviewMesh", "PreviewMesh", "Rectangular Light Preview Mesh | 矩形灯预览网格", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            Surface iSrf = null;
            Curve iCrv = null;
            double iDivideLength = 4;
            double iWidth = 0.2;
            double iOffsetDist = 0.2;
            double iIntensity = 100;
            Color iColor = Color.Red;

            DA.GetData("run", ref iRun);
            DA.GetData("srf", ref iSrf);
            DA.GetData("crv", ref iCrv);
            DA.GetData("lightLength", ref iDivideLength);
            DA.GetData("lightWidth", ref iWidth);
            DA.GetData("offsetDist", ref iOffsetDist);
            DA.GetData("intensity", ref iIntensity);
            DA.GetData("color", ref iColor);

            RectLight rectLight = new RectLight(iRun, iSrf, iCrv, iDivideLength, iWidth, iOffsetDist, iIntensity, iColor);
            rectLight.CreateOnSrf_DivideCurve();
            rectLight.CreateOnSrf_AdjustPlane();
            rectLight.CreateOnSrf_DefineLightGeometry();
            DA.SetDataList("PreviewMesh", rectLight.GetRectLightPreviewMesh());
            rectLight.CreateRectLight();

            DA.SetDataList("Segments", rectLight.CreateOnSrf_DivideCurve());
            DA.SetDataList("Points", rectLight.CreateOnSrf_GetDividePoint());
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resource.RectLightOnSrf;
        public override Guid ComponentGuid => new Guid("cc83945c-f86d-4c09-9ce2-0ce323f1b80a");
    }
    public class RecLightFitPlaneSrfComponent : GH_Component
    {
        public RecLightFitPlaneSrfComponent()
          : base("RectLightFitPlaneSrf", "RectLightFitPlaneSrf",
            "Create Rectangular Light fit to Base Plane Surface.Light Direction is equal to Surface Normal Direction.| 生成贴合平面曲面的对应曲面法线方向的矩形灯",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | 开关", GH_ParamAccess.item, false);
            pManager.AddSurfaceParameter("srf", "srf", "Base Surface | 基准面", GH_ParamAccess.item);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | 灯光亮度", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | 灯光颜色", GH_ParamAccess.item, Color.Red);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("PreviewMesh", "PreviewMesh", "Rectangular Light Preview Mesh | 矩形灯预览网格", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            Surface iSrf = null;
            double iIntensity = 100;
            Color iColor = Color.Red;

            DA.GetData("run", ref iRun);
            DA.GetData("srf", ref iSrf);
            DA.GetData("intensity", ref iIntensity);
            DA.GetData("color", ref iColor);

            RectLight rectLight = new RectLight(iRun, iSrf, iIntensity, iColor);
            rectLight.CreateFitPlaneSrf_DefineGeometry();
            DA.SetDataList("PreviewMesh", rectLight.GetRectLightPreviewMesh());
            rectLight.CreateRectLight();
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resource.RectLightFitSrf;
        public override Guid ComponentGuid => new Guid("66417ac7-ae1b-4787-b242-b0ab9447471e");
    }



    public class PointLightComponent : GH_Component
    {

        public PointLightComponent()
          : base("PointLight", "PointLight",
            "Create Point Light from Points| 以基点生成球形灯",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | 开关", GH_ParamAccess.item, false);
            pManager.AddPointParameter("location", "location", "Location of PointLight | 球形灯基点位置", GH_ParamAccess.list, Plane.WorldXY.Origin);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | 灯光亮度", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | 灯光颜色", GH_ParamAccess.item, Color.White);
            pManager.AddNumberParameter("previewRadius", "previewRadius", "Point Light Preview Radius | 球形灯预览半径", GH_ParamAccess.item, 1);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("previewMesh", "previewMesh", "previewMesh", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            List<Point3d> iLocation = new List<Point3d>();
            double iIntensity = 100;
            Color iColor = Color.White;
            double iPreviewRadius = 1;

            DA.GetData("run", ref iRun);
            DA.GetDataList("location", iLocation);
            DA.GetData("intensity", ref iIntensity);
            DA.GetData("color", ref iColor);
            DA.GetData("previewRadius", ref iPreviewRadius);

            PointLight ptLight = new PointLight(iRun, iLocation, iIntensity, iColor, iPreviewRadius);
            DA.SetDataList("previewMesh", ptLight.GetLightPreviewMesh(iPreviewRadius));
            ptLight.CreatePointLight();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.PointLight;
        public override Guid ComponentGuid => new Guid("d6d6e393-c739-4c7f-9c71-8a7299040b03");
    }

    public class SelectLightByTypeComponent : GH_Component
    {
        public SelectLightByTypeComponent()
          : base("SelectLightByType", "SelectLightByType",
            "Select Light Object by the light Type in Rhino Document . | 选择文档内指定类型灯光物件",
            "LightCreator", "SelectLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("select", "select", "Select Light Objects | 选择灯光", GH_ParamAccess.item);
            pManager.AddIntegerParameter("lightType", "lightType", "Light Type(0:Rectangular Lights;1:SpotLights;2.Point lights;3.Linear lights;4.Directional lights) | 指定类型(0:矩形灯光;1:聚光灯;2:点光源;3:管状灯光;4:平行光)", GH_ParamAccess.item, 0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("guids", "guids", "The Guids of Light Objects. | 灯光guids", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iSelect = false;
            int iLightType = 0;

            DA.GetData("select", ref iSelect);
            DA.GetData("lightType", ref iLightType);


            LightType selLight = new LightType(iSelect, iLightType);

            

            DA.SetDataList("guids", selLight.SelectLightByType());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.SelectLightByType;
        public override Guid ComponentGuid => new Guid("7c34f901-167c-412d-a131-e470fe9c79e4");
    }
}
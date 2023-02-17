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
            "Create Rectangular Light from Base Surface and Base Curves.Light Direction is equal to Surface Normal Direction | �������ϸ������ߣ����ɶ�Ӧ���淨�߷���ľ��ε�",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ����", GH_ParamAccess.item,false);
            pManager.AddSurfaceParameter("srf", "srf", "Base Surface | ��׼��", GH_ParamAccess.item);
            pManager.AddCurveParameter("crv", "crv", "Base Curves | ��׼��", GH_ParamAccess.item);
            pManager.AddNumberParameter("lightLength", "lightLength", "Length of Rectangular Light | ���εƳ���", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("lightWidth", "lightWidth", "Width of Rectangular Light | ���εƿ���", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("offsetDist", "offsetDist", "Distance From Light to Surface |�ƹ⵽��׼��ƫ�ƾ���", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.item, Color.Red);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
            pManager.AddCurveParameter("Segments", "Segments", "Splited Base Curves | ���ƹⳤ�ȷֶεĻ�׼����", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points | �ֶε�", GH_ParamAccess.list);
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

            RectLight rLight = new RectLight(iRun, iSrf, iCrv, iDivideLength, iWidth, iOffsetDist, iIntensity, iColor);
            rLight.DivideCurve();
            rLight.AdjustPlane();
            rLight.CreateRectLightOnSrf();

            DA.SetDataList("Segments", rLight.DivideCurve());
            DA.SetDataList("Points", rLight.GetDividePoint());
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resource.RectLight;
        public override Guid ComponentGuid => new Guid("cc83945c-f86d-4c09-9ce2-0ce323f1b80a");
    }

    public class PointLightComponent : GH_Component
    {

        public PointLightComponent()
          : base("PointLight", "PointLight",
            "Create Point Light from Points| �Ի����������ε�",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ����", GH_ParamAccess.item);
            pManager.AddPointParameter("location", "location", "Location of PointLight | ���εƻ���λ��", GH_ParamAccess.list);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.item, Color.White);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            List<Point3d> iLocation = new List<Point3d>();
            double iIntensity = 100;
            Color iColor = Color.White;

            DA.GetData("run", ref iRun);
            DA.GetDataList("location", iLocation);
            DA.GetData("intensity", ref iIntensity);
            DA.GetData("color", ref iColor);

            PointLight ptLight = new PointLight(iRun, iLocation, iIntensity, iColor);
            ptLight.CreatePointLight();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.PointLight;
        public override Guid ComponentGuid => new Guid("d6d6e393-c739-4c7f-9c71-8a7299040b03");
    }

    public class SelectLightByTypeComponent : GH_Component
    {

        public SelectLightByTypeComponent()
          : base("SelectLightByType", "SelectLightByType",
            "Select Light Object by the light Type in Rhino Document . | ѡ���ĵ���ָ�����͵ƹ����",
            "LightCreator", "SelectLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("select", "select", "Select Light Objects | ѡ��ƹ�", GH_ParamAccess.item);
            pManager.AddIntegerParameter("lightType", "lightType", "Light Type(0:Rectangular Lights;1:SpotLights;2.Point lights;3.Linear lights;4.Directional lights) | ָ������(0:���εƹ�;1:�۹��;2:���Դ;3:��״�ƹ�;4:ƽ�й�)", GH_ParamAccess.item,0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("guids", "guids", "The Guids of Light Objects. | �ƹ�guids", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iSelect = false;
            int iLightType = 0;

            DA.GetData("select", ref iSelect);
            DA.GetData("lightType",ref iLightType);

            LightType selLight = new LightType(iSelect, iLightType);
            selLight.SelectLightByType();

            DA.SetDataList("guids", selLight.SelectLightByType());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.SelectLightByType;
        public override Guid ComponentGuid => new Guid("7c34f901-167c-412d-a131-e470fe9c79e4");
    }
}
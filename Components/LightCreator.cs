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
    public class RecLightOnSrfAlongCrvComponent : GH_Component
    {
        public RecLightOnSrfAlongCrvComponent()
          : base("RectLightOnSrfAlongCrv", "RectLightOnSrfAlongCrv",
            "Create Rectangular Light from Base Surface and Base Curves.Light Direction is equal to Surface Normal Direction.| �������ϸ������ߣ����ɶ�Ӧ���淨�߷���ľ��εƹ�",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ��ť", GH_ParamAccess.item, false);
            pManager.AddSurfaceParameter("srf", "srf", "Base Surface | ��׼��", GH_ParamAccess.item);
            pManager.AddCurveParameter("crv", "crv", "Base Curves | ��׼��", GH_ParamAccess.item);
            pManager.AddNumberParameter("lightLength", "lightLength", "Length of Rectangular Light | ���εƳ���", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("lightWidth", "lightWidth", "Width of Rectangular Light | ���εƿ��", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("offsetDist", "offsetDist", "Distance From Light to Surface |�ƹ⵽��׼��ƫ�ƾ���", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.list, Color.White);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            SolutionExpired += (sender, args) => {
                ((GH_Component)sender).Params.Input[1].DataMapping = GH_DataMapping.Flatten;
                ((GH_Component)sender).Params.Input[2].DataMapping = GH_DataMapping.Flatten;
            };
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddCurveParameter("Segments", "Segments", "Splited Base Curves | ���ƹⳤ�ȷֶεĻ�׼����", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points | �ֶε�", GH_ParamAccess.list);
            pManager.AddCurveParameter("PreviewCrvs", "PreviewCrvs", "Rectangular Light silhoutte preview | ���ε�������Ԥ��", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            Surface iSrf = null;
            Curve iCrv = null;
            double iLightLength = 4;
            double iLightWidth = 0.2;
            double iOffsetDist = 0.2;
            double iIntensity = 100;
            List<Color> iColor = new List<Color>();

            DA.GetData("run", ref iRun);
            DA.GetData("srf", ref iSrf);
            DA.GetData("crv", ref iCrv);
            DA.GetData("lightLength", ref iLightLength);
            DA.GetData("lightWidth", ref iLightWidth);
            DA.GetData("offsetDist", ref iOffsetDist);
            DA.GetData("intensity", ref iIntensity);
            DA.GetDataList("color", iColor);

            RectLight rectLight = new RectLight(iRun, iSrf, iCrv, iLightLength, iLightWidth, iOffsetDist, iIntensity, iColor);
            rectLight.CreateOnSrfAlongCrv_DivideCurve();
            rectLight.CreateOnSrfAlongCrv_AdjustPlane();
            if (rectLight.PlnList.Count != iColor.Count&&iColor.Count>1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
            }
            rectLight.CreateOnSrfAlongCrv_DefineLightGeometry();
            DA.SetDataList("PreviewCrvs", rectLight.CreateOnSrfAlongCrv_GetRectLightPreviewCrvs());
            rectLight.CreateRectLight();

            DA.SetDataList("Segments", rectLight.CreateOnSrfAlongCrv_DivideCurve());
            DA.SetDataList("Points", rectLight.CreateOnSrfAlongCrv_GetDividePoint());
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resource.RectLightOnSrfAlongCrv;
        public override Guid ComponentGuid => new Guid("cc83945c-f86d-4c09-9ce2-0ce323f1b80a");
    }
    public class RecLightFitPlaneSrfComponent : GH_Component
    {
        public RecLightFitPlaneSrfComponent()
          : base("RectLightFitPlaneSrf", "RectLightFitPlaneSrf",
            "Create Rectangular Light fit to Base Rectangular Plane Surface.Light Direction is equal to Surface Normal Direction.| ���ɴ�С���Ͼ���ƽ�������Ե,���ҷ���Ϊ��Ӧ���淨�߷���ľ��εƹ�",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ��ť", GH_ParamAccess.item, false);
            pManager.AddSurfaceParameter("srf", "srf", "Base Surface | ��׼��", GH_ParamAccess.list);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.list, Color.White);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            SolutionExpired += (sender, args) => {
                ((GH_Component)sender).Params.Input[1].DataMapping = GH_DataMapping.Flatten;
            };
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("PreviewRect", "PreviewRect", "Rectangular Light Preview silhoutte | ���ε�Ԥ��������", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            List<Surface> iSrf = new List<Surface>();
            double iIntensity = 100;
            List<Color> iColor = new List<Color>();

            DA.GetData("run", ref iRun);
            DA.GetDataList("srf", iSrf);
            DA.GetData("intensity", ref iIntensity);
            DA.GetDataList("color", iColor);

            RectLight rectLight = new RectLight(iRun, iSrf, iIntensity, iColor);
            if (rectLight.BaseSrfList.Count != iColor.Count && iColor.Count > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
            }
            rectLight.CreateFitPlaneSrf_DefineGeometry();

            DA.SetDataList("PreviewRect", rectLight.CreateFitPlaneSrf_GetRectLightPreviewCrvs());
            rectLight.CreateRectLight();
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resource.RectLightFitSrf;
        public override Guid ComponentGuid => new Guid("66417ac7-ae1b-4787-b242-b0ab9447471e");
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
            pManager.AddBooleanParameter("run", "run", "Button | ��ť", GH_ParamAccess.item, false);
            pManager.AddPointParameter("point", "point", "Base point of PointLight | ���εƻ���λ��", GH_ParamAccess.list, Plane.WorldXY.Origin);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.list, Color.White);
            pManager.AddNumberParameter("previewRadius", "previewRadius", "Point Light Preview Radius | ���ε�Ԥ���뾶", GH_ParamAccess.item, 1);
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            SolutionExpired += (sender, args) => {
                ((GH_Component)sender).Params.Input[1].DataMapping = GH_DataMapping.Flatten;
            };
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
            List<Color> iColor = new List<Color>();
            double iPreviewRadius = 1;

            DA.GetData("run", ref iRun);
            DA.GetDataList("point", iLocation);
            DA.GetData("intensity", ref iIntensity);
            DA.GetDataList("color", iColor);
            DA.GetData("previewRadius", ref iPreviewRadius);

            PointLight ptLight = new PointLight(iRun, iLocation, iIntensity, iColor, iPreviewRadius);
            if (ptLight.Location.Count != iColor.Count && iColor.Count > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
            }
            ptLight.Default_DefinePointLightGeometry();
            DA.SetDataList("previewMesh", ptLight.Default_GetPointLightPreviewMesh());
            ptLight.CreatePointLight();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.PointLight;
        public override Guid ComponentGuid => new Guid("d6d6e393-c739-4c7f-9c71-8a7299040b03");
    }

    public class SpotLightAlongLineComponent : GH_Component
    {

        public SpotLightAlongLineComponent()
          : base("SpotLightAlongLine", "SpotLightAlongLine",
            "Create SpotLight along lines direction.| ���Ŷ��ֱ�ߵķ������ɾ۹�ơ�",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ��ť", GH_ParamAccess.item, false);
            pManager.AddLineParameter("line", "line", "Base direction line of SpotLight | �۹�Ʒ�����", GH_ParamAccess.list);
            pManager.AddNumberParameter("spotLightAngle", "spotLightAngle", "SpotLight Beam Angle.NOTICE:PLEASE INPUT A NUMBER LARGER THAN 0 AND SMALLER THAN 90. | �۹�ƹ�����.ע�⣺������0-90֮�����ֵ��", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.list, Color.White);
            
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            SolutionExpired += (sender, args) => {
                ((GH_Component)sender).Params.Input[1].DataMapping = GH_DataMapping.Flatten;
            };
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("PreviewCone", "previewCone", "SpotLight Preview Cone Mesh | �۹��Ԥ��Բ׶����", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            List<Line> iLine = new List<Line>();
            double iSpotLightAngle = 30;
            double iIntensity = 100;
            List<Color> iColor = new List<Color>();

            DA.GetData("run", ref iRun);
            DA.GetDataList("line", iLine);
            DA.GetData("spotLightAngle", ref iSpotLightAngle);
            DA.GetData("intensity", ref iIntensity);
            DA.GetDataList("color", iColor);

            SpotLight spotLight = new SpotLight(iRun, iLine, iSpotLightAngle, iIntensity, iColor);
            if (spotLight.BaseLine.Count != iColor.Count && iColor.Count > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
            }
            spotLight.CreateAlongLine_DefineSpotLightGeometry();
            DA.SetDataList("PreviewCone", spotLight.CreateAlongLine_GetSpotLightPreviewMesh());
            spotLight.CreateSpotLight();
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.SpotLightAlongLine;
        public override Guid ComponentGuid => new Guid("3e5621cf-fa99-46b0-ac3b-6de6e67bcbf7");
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
            pManager.AddIntegerParameter("lightType", "lightType", "Light Type(0:Rectangular Lights;1:SpotLights;2.Point lights;3.Linear lights;4.Directional lights) | ָ������(0:���εƹ�;1:�۹��;2:���Դ;3:��״�ƹ�;4:ƽ�й�)", GH_ParamAccess.item, 0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Guids", "Guids", "The Guids of Light Objects. | �ƹ�GUID", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iSelect = false;
            int iLightType = 0;

            DA.GetData("select", ref iSelect);
            DA.GetData("lightType", ref iLightType);


            LightType selLight = new LightType(iSelect, iLightType);



            DA.SetDataList("Guids", selLight.SelectLightByType());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.SelectLight;
        public override Guid ComponentGuid => new Guid("7c34f901-167c-412d-a131-e470fe9c79e4");
    }
    public class EditLightComponent : GH_Component
    {
        public EditLightComponent()
          : base("EditLight", "EditLight",
            "Edit LightObject's Attributes. | �༭�ƹ����������",
            "LightCreator", "EditLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ��ť", GH_ParamAccess.item, false);
            pManager.AddParameter(new Param_Guid(), "lightGuids", "lightGuids", "The Guids of Light Objects | �ƹ�Guids", GH_ParamAccess.list);
            pManager.AddNumberParameter("lightLength", "lightLength", "Length of Rectangular Light | ���εƳ���", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("lightWidth", "lightWidth", "Width of Rectangular Light | ���εƿ��", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.list, Color.White);
            pManager.AddBooleanParameter("isRandomColor", "isRandomColor", "LightObject's color is Random Color | �ƹ������ɫ", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("rotateAngle", "rotateAngle", "Light Rotation Angle after click run button.| ���°�ť�󣬵ƹⵥ����ת�Ƕ�", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("spotLightAngle", "spotLightAngle", "When input Light Object are SpotLight,it changes SpotLight angle.| ������ƹ�Ϊ�۹��ʱ,�޸ľ۹�ƽǶ�", GH_ParamAccess.item, 0);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("randomColors", "randomColors", "Random Color RGB code | �����ɫ�����еƹ��RGB����", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            List<Guid> iLightGuids = new List<Guid>();
            double iLightLength = double.NaN;
            double iLightWidth = double.NaN;
            double iIntensity = 100;
            List<Color> iColor = new List<Color>();
            bool iIsRandomColor = false;
            double iRotateAngle = 0;
            double iSpotLightAngle = 0;

            DA.GetData("run", ref iRun);
            DA.GetDataList("lightGuids", iLightGuids);
            DA.GetData("lightLength", ref iLightLength);
            DA.GetData("lightWidth", ref iLightWidth);
            DA.GetData("intensity", ref iIntensity);
            DA.GetDataList("color", iColor);
            DA.GetData("isRandomColor", ref iIsRandomColor);
            DA.GetData("rotateAngle", ref iRotateAngle);
            DA.GetData("spotLightAngle", ref iSpotLightAngle);

            LightEditor lightEditor = new LightEditor(iRun, iLightGuids, iLightLength, iLightWidth, iIntensity, iColor, iIsRandomColor, iRotateAngle,iSpotLightAngle);
            lightEditor.EditLightAttributes();

            lightEditor.RotateLightObject();

            DA.SetDataList("randomColors", lightEditor.GetRandomColorList());
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resource.EditLight;
        public override Guid ComponentGuid => new Guid("80b4b495-c7b6-42e0-8dd6-cdc31194c1e6");
    }
    public class LightDivideCrvComponent : GH_Component
    {
        public LightDivideCrvComponent()
          : base("LightDivideCrv", "LightDivideCrv",
            "Create Rectangular Light/Point Light/Spot Light from base curve's divide Point." +
                "Light direction is Z axis of divide points' plane." +
                "----------------------------------" +
                "�������ϸ������ߵķָ�㣬���ɾ��εơ����Դ�;۹�ơ�" +
                "��ķ����Ӧ���߷ָ��ƽ���z�ᡣ" +
                "countMode�����������������ָ�,Ĭ��ģʽ���Ծ���ָ" +
                "lightMode = 0Ϊ���εƣ�1Ϊ���Դ��2Ϊ�۹��",
            "LightCreator", "CreateLight")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "Button | ��ť", GH_ParamAccess.item, false);
            pManager.AddCurveParameter("crv", "crv", "Base Curves | ��׼��", GH_ParamAccess.item);
            
            pManager.AddIntegerParameter("lightMode", "lightMode", "��Դģʽ | Light Mode. 0 = Rectangular Light/���ε�,1 = Point Light/���Դ,2 = Spotlight/�۹��", GH_ParamAccess.item, 0);
            if (pManager[2] is Param_Integer param_Integer)
            {
                param_Integer.AddNamedValue("RectLight", 0);
                param_Integer.AddNamedValue("PointLight", 1);
                param_Integer.AddNamedValue("SpotLight", 2);
            }

            

            pManager.AddBooleanParameter("countMode", "countMode", "When countMode is on,the base curve is divided by count.Default mode is divide Length. | countMode�����������������ָ�,Ĭ��ģʽ���Ծ���ָ�", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("divideCount", "divideCount", "When countMode is on,this parameter controls the count of divide points | countMode�����󣬿��Ʒָ������", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("divideLength", "divideLength", "Divide Length | �ָ�����", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("lightLength", "lightLength", "Length of Rectangular Light.It also can change spotLight Length. | ���εƳ��ȣ�Ҳ���Ըı�۹�Ƶĳ���", GH_ParamAccess.item, 3);
            pManager.AddNumberParameter("lightWidth", "lightWidth", "Width of Rectangular Light | ���εƿ��", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("intensity", "intensity", "Light Intensity | �ƹ�����", GH_ParamAccess.item, 100);
            pManager.AddColourParameter("color", "color", "Light Color | �ƹ���ɫ", GH_ParamAccess.list, Color.White);
            pManager.AddNumberParameter("rectLightAngle", "rectLightAngle", "When switch to rectangular light mode,this parameter controls the rotate angle of rectangular light.|���ε�ģʽ�£����ƾ��ε���ת�Ƕ�", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("spotLightAngle", "spotLightAngle", "When switch to spot light mode,this parameter controls the beam angle of spotlight.NOTICE:PLEASE INPUT A NUMBER LARGER THAN 0 AND SMALLER THAN 90.|�۹��ģʽ�£����ƾ۹�ƹ�����.ע�⣺������0-90֮�����ֵ��", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("lightRadius", "lightRadius", "When switch to Point Light Mode,this parameter controls the radius of point light preview sphere.When switch to spotlight Mode,this parameter controls the radius of spotlight.|���Դģʽ�£�����Ԥ������뾶��С��", GH_ParamAccess.item, 2);
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            SolutionExpired += (sender, args) => {
                ((GH_Component)sender).Params.Input[1].DataMapping = GH_DataMapping.Flatten;
            };
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("PreviewRect", "PreviewRect", "Rectangular lights' silhoutte preview | ���ε�������Ԥ��", GH_ParamAccess.list);
            pManager.AddLineParameter("PreviewRectDir", "PreviewRectDir", "Rectangular lights' direction preview | ���εƷ���Ԥ��", GH_ParamAccess.list);
            pManager.AddMeshParameter("PreviewSphere", "PreviewSphere", "Point Light sphere preview mesh. | ���Դ��������Ԥ��", GH_ParamAccess.list);
            pManager.AddMeshParameter("PreviewCone", "PreviewCone", "Spotlight Cone preview mesh. | �۹��Բ׶������Ԥ��", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Light's base plane. | �ƹⲼ��ƽ��", GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iRun = false;
            Curve iCrv = null;
            int iLightMode = 0;
            bool iCountMode = false;
            double iLightRadius = 5;
            int iDivideCount = 5;
            double iDivideLength = 5;
            double iLightLength = 3;
            double iLightWidth = 0.2;
            double iIntensity = 100;
            List<Color> iColor = new List<Color>();
            double iRectLightAngle = 0;
            double iSpotLightAngle = 0;

            DA.GetData("run", ref iRun);
            DA.GetData("crv", ref iCrv);
            DA.GetData("lightMode", ref iLightMode);
            DA.GetData("countMode", ref iCountMode);
            DA.GetData("divideCount", ref iDivideCount);
            DA.GetData("divideLength", ref iDivideLength);
            DA.GetData("lightLength", ref iLightLength);
            DA.GetData("lightWidth", ref iLightWidth);
            DA.GetData("intensity", ref iIntensity);
            DA.GetDataList("color", iColor);
            DA.GetData("rectLightAngle", ref iRectLightAngle);
            DA.GetData("spotLightAngle", ref iSpotLightAngle);
            DA.GetData("lightRadius", ref iLightRadius);



            if (iLightMode == 0)
            {
                RectLight rectLight = new RectLight(iRun, iCrv, iLightMode, iCountMode, iDivideCount, iDivideLength, iLightLength, iLightWidth, iIntensity, iColor, iRectLightAngle);
                rectLight.CreateOnCrv_DivideCurve();
                rectLight.CreateOnCrv_GetDividePoint();

                DA.SetDataList("Plane", rectLight.CreateOnCrv_GetPlane());
                if (rectLight.PlnList.Count != iColor.Count && iColor.Count > 1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
                }
                rectLight.CreateOnCrv_DefineRectLightGeometry();

                DA.SetDataList("PreviewRect", rectLight.CreateOnCrv_GetRectLightPreviewCrvs());
                DA.SetDataList("PreviewRectDir", rectLight.CreateOnCrv_GetDirectionLine());
                if (iRun == true)
                {
                    rectLight.CreateRectLight();
                }
            }

            if (iLightMode == 1)
            {
                PointLight pointLight = new PointLight(iRun, iCrv, iLightMode, iCountMode, iLightRadius, iDivideCount, iDivideLength, iLightLength, iLightWidth, iIntensity, iColor);
                pointLight.CreateOnCrv_DivideCurve();
                pointLight.CreateOnCrv_GetDividePoint();
                DA.SetDataList("Plane", pointLight.CreateOnCrv_GetPlane());
                if (pointLight.ParamList.Count != iColor.Count && iColor.Count > 1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
                }
                pointLight.CreateOnCrv_DefinePointLightGeometry();

                DA.SetDataList("PreviewSphere", pointLight.CreateOnCrv_GetPointLightPreviewMesh());
                if (iRun == true)
                {
                    pointLight.CreatePointLight();
                }

            }

            if (iLightMode == 2)
            {
                SpotLight spotLight = new SpotLight(iRun, iCrv, iLightMode, iCountMode, iDivideCount, iDivideLength, iLightLength, iIntensity, iColor, iSpotLightAngle);
                spotLight.CreateOnCrv_DivideCurve();
                spotLight.CreateOnCrv_GetDividePoint();
                DA.SetDataList("Plane", spotLight.CreateOnCrv_GetPlane());
                if (spotLight.PlnList.Count != iColor.Count && iColor.Count > 1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Color's Count is not equal to Light's Count | ��ɫ�����͵ƹ�������ƥ��");
                }
                spotLight.CreateOnCrv_DefineSpotLightGeometry();

                DA.SetDataList("PreviewCone", spotLight.CreateOnCrv_GetSpotLightPreviewMesh());
                if (iRun == true)
                {
                    spotLight.CreateSpotLight();
                }

            }
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resource.LightDividePts;
        public override Guid ComponentGuid => new Guid("776717cd-8636-4665-869b-6b25d04b6e7d");
    }
}
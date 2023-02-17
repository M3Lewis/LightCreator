using Grasshopper;
using Grasshopper.Kernel;
using LightCreator.Properties;
using System;
using System.Drawing;

namespace LightCreator
{
    public class LightCreatorInfo : GH_AssemblyInfo
    {
        public override string Name => "LightCreator";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resource.LightCreator;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("a6ffc90b-d353-4ff3-b504-63b442693685");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        
    }
}
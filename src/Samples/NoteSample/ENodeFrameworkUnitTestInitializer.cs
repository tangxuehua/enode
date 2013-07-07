using System.Reflection;
using ENode;

namespace NoteSample
{
    public class ENodeFrameworkUnitTestInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            //全部使用默认配置，一般单元测试时，可以使用该配置
            Configuration.StartWithAllDefault(new Assembly[] { Assembly.GetExecutingAssembly() });
        }
    }
}

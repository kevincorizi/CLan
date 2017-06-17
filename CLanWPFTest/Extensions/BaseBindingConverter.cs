using System.Windows.Markup;

namespace CLanWPFTest.Extensions
{
    public abstract class BaseBindingConverter : MarkupExtension
    {
        public override object ProvideValue(System.IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

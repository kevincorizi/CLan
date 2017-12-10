using System.Windows.Markup;

namespace CLan.Extensions
{
    public abstract class BaseBindingConverter : MarkupExtension
    {
        public override object ProvideValue(System.IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

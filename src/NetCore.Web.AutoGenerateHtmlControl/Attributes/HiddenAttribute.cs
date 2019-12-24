namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    public class HiddenAttribute : FormControlsAttribute
    {
        public HiddenAttribute()
        {
            ControlType = HtmlControlType.Hidden;
        }
    }
}

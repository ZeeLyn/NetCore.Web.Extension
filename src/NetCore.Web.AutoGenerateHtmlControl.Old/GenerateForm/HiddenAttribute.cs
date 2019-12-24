namespace NetCore.Web.AutoGenerateHtmlControl.GenerateForm
{
    public class HiddenAttribute : FormControlsAttribute
    {
        public HiddenAttribute()
        {
            ControlType = HtmlControl.Hidden;
        }
    }
}

using System.Web.UI.HtmlControls;

namespace ABTesting.Controls
{
    public class Alternative : HtmlContainerControl
    {
        public string Name { get; set; }
        public bool RenderSilently { get; set; }

        protected override void RenderBeginTag(System.Web.UI.HtmlTextWriter writer)
        {
            if (!RenderSilently)
            {
                base.RenderBeginTag(writer);
            }
        }

        protected override void RenderEndTag(System.Web.UI.HtmlTextWriter writer)
        {
            if (!RenderSilently)
            {
                base.RenderEndTag(writer);
            }
        }

    }
}

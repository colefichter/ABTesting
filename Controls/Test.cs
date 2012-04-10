using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ABTesting.Controls
{
	[ControlBuilder(typeof(ABTestBuilder)), ParseChildren(false)]
	public class Test : WebControl, INamingContainer
	{
		public string TestName { get; set; }

		protected override void Render(HtmlTextWriter writer)
		{
			if (Controls.Count == 0)
			{
				return;
			}

			Experiment test = FairlyCertain.GetOrCreateTest(TestName, Controls);
			ABAlternative choice = FairlyCertain.GetUserAlternative(test);

			Controls[choice.Index].RenderControl(writer);
		}
	}

	internal class ABTestBuilder : ControlBuilder
	{
		public override Type GetChildControlType(
		   string tagName, IDictionary attributes)
		{
			if (tagName == "ABAlternative")
				return typeof(Alternative);

			return null;
		}

		public override void AppendLiteralString(string s)
		{
		}
	}
}

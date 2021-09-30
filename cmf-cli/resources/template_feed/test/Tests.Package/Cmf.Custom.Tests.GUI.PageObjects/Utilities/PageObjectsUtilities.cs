using Cmf.Core.Controls.PageObjects.Components;
using Cmf.Core.PageObjects;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customization.Common.PageObjects.Utilities
{
	/// <summary>
	/// 
	/// </summary>
	public class PageObjectsUtilities
	{
		/// <summary>
		/// Click on a PageObjectn Open a wizard, complete all the steps and closes the wizard
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="wizardOpener"></param>
		/// <param name="selector"></param>
		/// <returns>success</returns>
		public static bool OpenWizardAndComplete<T>(BasePageObject wizardOpener, string selector = null) where T : Wizard
		{
			wizardOpener.Click();

			var wizard = wizardOpener.CreatePageObject<T>(selector ?? Wizard.Selector, searchContext: wizardOpener.Driver);
			BaseTestClass.WaitForLoadingStop();

			var hasSteps = wizard.ElementExistsInDom(By.CssSelector(HorizontalStepList.Selector));
			if (hasSteps)
			{
				var len = wizard.Steps.Items.Count - 1;
				for (int i = 0; i < len; i++)
				{
					wizard.ClickNextButton();
					BaseTestClass.WaitForLoadingStop();
				}
			}
			return wizard.ClickFinishButtonAndWaitForResult();
		}
	}
}

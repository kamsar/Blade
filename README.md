Blade
=====

Blade is a presentation framework for Sitecore 7. It is not compatible with Sitecore 6.x.

## How Blade Works

Blade uses the [MVP pattern](http://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93presenter) to allow you to create very DRY, single-responsibility renderings; focus on presentation code and not data access by coding renderings against interface contracts or ViewModels instead of the Sitecore API.

With Blade, your renderings only request the type of model they expect. For example:

    public partial class SampleSublayout : UserControlView<MyViewModelClass>
	
Then Blade steps in and maps the model type onto a Presenter class, which defines how to supply the model type. For example:

	public class SamplePresenter : SitecorePresenter<MyViewModelClass>
	{
		protected override MyViewModelClass GetModel(IView view, Item dataSource)
		{
			var model = new MyViewModelClass();
			model.Example = dataSource.DisplayName;
			
			return model;
		}
	}
	
This enables simple isolation of both views and presenters for writing testable frontends, and looser coupling even if you aren't testing. Don't worry, not every rendering needs a presenter. If you're just using a model type from a Sitecore object mapper such as Synthesis, Glass, or Compiled Domain Model, you can make that automatically map to the data source item without defining an explicit presenter (if you're curious, check out the code for [Synthesis.Blade's](/kamsar/Synthesis/blob/master/Source/Synthesis.Blade/Configuration/SynthesisPresenterFactory.cs) `PresenterFactory`).

## Ubiquitous Razor 2 Support

Blade also brings first-class support of the Razor 2 templating language into Sitecore, without using the Sitecore MVC functionality. This is advantageous when developing sites that use modules that are not compatible with the Sitecore MVC rendering technology - you can reap the benefits of MVC-style renderings without the hassle of implementing renderings twice or resorting to XSLT. A Razor rendering with Blade works exactly like a sublayout - only the path is to a cshtml file instead of an ascx. There are also many useful helpers included that simplify things like rendering dates, images, and controlling page editor functionality. For example:

	@inherits RazorRendering<MyProject.MyViewModelClass>
	
	<h1>@Html.Raw(Model.Example)</h1>
	
	<p>@Html.TextFor(model => model.SitecoreDataField)</p>

Blade does its best to feel as much as possible like Sitecore as well as ASP.NET MVC, so it doesn't require digesting lots of unfamiliar ways of doing things. Sitecore features such as Page Editor are fully supported. MVC conventions like automatic HTML encoding, partial views, anonymous types defining HTML attributes, and relative view referencing are all there.

## Awesome Rendering Diagnostics

Ever spent an hour figuring out why a rendering was busted, only to realize it was output cached without you realizing it? What about having to find a rendering file for a HTML/CSS dev so they can make markup changes? You'll love Blade then. Blade's rendering types emit HTML comments that indicate where a specific rendering begins and ends, as well as if it was output cached, when, and what the cache criteria were. The comments disappear when dynamic debug compilation is disabled, leaving production sites pristine.

Check this out:

	<!-- Begin Rendering ~/layouts/Sample Inner Sublayout.ascx -->
	<!-- Rendering was output cached at 5/16/2013 9:31:42 PM, ClearOnIndexUpdate, VaryByData, VaryByQueryString -->
	<h1>Hello, world!</h1>
	<!-- End Rendering ~/layouts/Sample Inner Sublayout.ascx, render took 0ms -->

## Full Sitecore 7 Support

Blade supports the enhancements made to renderings in Sitecore 7. It supports renderings that decache on index update, have multiple items as data sources, index-query based data sources, and even defining your own data source resolution logic. Even better, it removes boilerplate code by automatically normalizing all kinds of data source specifications back into simple `Item` (or `Item[]`) instances.

## What next?

See the wiki for more detailed documentation, or just go get the package off NuGet and play around. Make sure to install the samples package if it's your first time :)

## Why "Blade"?

You may be thinking "because it has Razor support, how...uh...clever." You'd be partially right, but it's really a [ridiculous movie reference](https://www.google.com/search?&q=razor+and+blade+hackers&tbm=isch) ;)
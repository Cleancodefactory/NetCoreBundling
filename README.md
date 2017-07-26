# NetCoreBundling#
A .Net Core rewrite and extension of the former Asp.Net Web Optimization bundling framework

## Why NetCoreBundling ##
Microsoft's Asp.Net Web Optimization bundling framework offered a convenient way for configuring bundles in code. Depending on the environment Debug/Release it was possible to serve the resources in different forms (e.g. minified or not).
You will say it is possible in .NET Core as well by using Gulp tasks. It is indeed possible but Gulp needs NodeJs. What if your provider didn't let you install NodeJs? Do you have to do the minification all during deployment? What if you want to debug some Javascript on site but don't want to deploy a debug version first?
For all these cases we are providing NetCoreBundling.

## Getting started ##
To use NetCoreBundling you have to install the NuGet-Package from here: NUGET-PACKAGE

### Simple usage ###
- Init the BundleCollection in Configure:
```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DiagnosticListener diagnosticListener)
{
	//Bundling
	bool useOptimization = !env.IsDevelopment();/*Minify and optimize whatever possible*/
	BundleCollection bundleCollection = app.UseBundling(env, loggerFactory.CreateLogger("Bundling"), "res", useOptimization);
	bundleCollection.EnableInstrumentations = env.IsDevelopment(); //Logging enabled

	//Configure bundles
	bundleCollection.Add(new StyleBundle("cssbundle").
	Include(@"~/css/css1.css", @"~/css/css2.css").
	IncludeCdn(new CdnObject { CdnPath = "//maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css", Integrity= "sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u", Crossorigin= "anonymous" }));
	bundleCollection.Add(new ScriptBundle("jsbundle").
	Include(@"~/js/test1.js", @"~/js/test2.js"));
	//Bundle dirCssBundle = new StyleBundle("~/My/CssDirFiles").IncludeDirectory("/css", "*.css", true);
	//BundleCollection.Instance.Add(dirCssBundle);
	//Bundle dirJsBundle = new ScriptBundle("~/My/JsDirFiles").IncludeDirectory("/js", "*.js", true);
	//BundleCollection.Instance.Add(dirJsBundle);
}
```

- Render the configured bundles:  
In your master page or razor view you have to call the created bundles:
```cs
@using ccf.CoreKraft.Web.Bundling
<!DOCTYPE html>
<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>_Layout</title>    
    @BundleCollection.Instance.Styles.Render()
</head>
<body>
    @RenderBody()   
    @BundleCollection.Instance.Scripts.Render()
</body>
</html>
```
- Use the default transformations

### Advanced usage ###

## License ##
This project is licensed under the Apache License. This means that you can use, modify and distribute it freely. See [http://www.apache.org/licenses/LICENSE-2.0.html](http://www.apache.org/licenses/LICENSE-2.0.html) for more details.
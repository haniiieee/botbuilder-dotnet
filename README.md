# Bot Builder SDK v4

This repository contains code for the .NET version of the [Microsoft Bot Builder SDK](https://github.com/Microsoft/botbuilder-dotnet).
The 4.x version of the SDK is being actively developed and should therefore be used for **EXPERIMENTATION PURPOSES ONLY**.
Production bots should continue to be developed using the [v3 SDK](https://github.com/Microsoft/BotBuilder/tree/master/CSharp).

In addition to the .NET SDK, Bot Builder supports creating bots in other popular programming languages:

- The [v4 JavaScript SDK](https://github.com/Microsoft/botbuilder-js) has a high degree of parity with the .NET SDK 
  and lets you build rich bots using JavaScript for the Microsoft Bot Framework.
- The [Python Connector](https://github.com/Microsoft/botbuilder-python) provides basic connectivity to the Microsoft Bot Framework 
  and lets developers build bots using Python. **v4 SDK coming soon**.
- The [Java Connector](https://github.com/Microsoft/botbuilder-java) provides basic connectivity to the Microsoft Bot Framework 
  and lets developers build bots using Java. **v4 SDK coming soon**.

To see our [roadmap](https://github.com/Microsoft/botbuilder-js/blob/master/FAQ.md#q-is-there-a-roadmap-for-this-sdk-when-will-this-be-generally-available) for the v4 SDK and other common questions, consult our [FAQ](FAQ.md).

## Getting Started

The v4 SDK consists of a series of [libraries](/libraries) which can be installed.

<!--Include detailed instructions on how to install the libraries.-->

### Create a "Hello World" bot

Create a new **ASP.NET Core Web Application**:
- Target **.NET Core** **ASP.NET Core 2.0**.
- Pick the **Web API** project template.
- Select **No Authentication**.

Add the following NuGet packages to your project:
```
Microsoft.Bot.Builder
Microsoft.Bot.Builder.BotFramework
Microsoft.Bot.Connector
Microsoft.Bot.Schema
```
Update the Startup.cs file to this:
```
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Samples
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
```
The Web API template includes a prebuilt Values controller. By convention bots use /api/messages, so rename ValuesController to MessagesController and add code as follows:
```
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Samples
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter _adapter;

        public MessagesController(IConfiguration configuration)
        {
            var bot = new Builder.Bot(new BotFrameworkAdapter(configuration));
            _adapter = (BotFrameworkAdapter)bot.Adapter;
            bot.OnReceive(async (context, next) =>
            {
                if (context.Request.Type == ActivityTypes.Message)
                {
                    context.Reply($"Hello World");
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await _adapter.Receive(this.Request.Headers["Authorization"].FirstOrDefault(), activity);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
        }
    }
}
```

Now start your bot (with or without debugging).

To interact with your bot:
- Download the [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator).
- The start the emulator, connect to your bot, and say "hello" and the bot will respond with "Hello World" to every message.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Reporting Security Issues
Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) at [secure@microsoft.com](mailto:secure@microsoft.com). You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the [MSRC PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found in the [Security TechCenter](https://technet.microsoft.com/en-us/security/default).

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.

<!--
## Current Build Status
| Project  | Status |
| --- | --- |
| Microsoft.Bot.Connector | ![Build Status](https://fuselabs.visualstudio.com/_apis/public/build/definitions/86659c66-c9df-418a-a371-7de7aed35064/212/badge) |
-->

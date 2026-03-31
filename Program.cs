using clap_listener.Options;
using clap_listener.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AudioCaptureOptions>(builder.Configuration.GetSection(AudioCaptureOptions.SectionName));
builder.Services.Configure<ClapDetectionOptions>(builder.Configuration.GetSection(ClapDetectionOptions.SectionName));
builder.Services.Configure<DoubleClapOptions>(builder.Configuration.GetSection(DoubleClapOptions.SectionName));
builder.Services.Configure<ActionOptions>(builder.Configuration.GetSection(ActionOptions.SectionName));

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Clap Listener";
});

builder.Services.AddSingleton<IAudioCapture, NAudioCaptureService>();
builder.Services.AddSingleton<IClapDetector, ClapDetector>();
builder.Services.AddSingleton<IDoubleClapDetector, DoubleClapDetector>();
builder.Services.AddSingleton<IActionExecutor, ProcessActionExecutor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

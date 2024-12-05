# BilibiliMonitor
使用示例：
```csharp
Config config = new("Config.json");
config.LoadConfig();

Dynamics.OnDynamicUpdated += OnDynamicUpdated;
LiveStreams.OnLiveStreamUpdated += OnLiveStreamUpdated;
Bangumi.OnBanguimiUpdated += OnBanguimiUpdated;
Bangumi.OnBanguimiEnded += OnBanguimiEnded;

private void OnDynamicUpdated(DynamicModel.Item item, long uid, string picPath)
{
     Console.WriteLine($"{item.modules.module_author.name} 更新了动态, https://t.bilibili.com/{item.id_str}");
     Console.WriteLine(picPath);
}
```

# BilibiliMonitor
使用 SkiaSharp 绘图，支持自动 Cookie 刷新。
使用前请先获取哔哩哔哩的 Cookie 以及`ac_time_value`，后者来源于哔哩哔哩的 localStorage；若发现没有，尝试进行一次登出后重新登录

## 使用示例
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

## 示例
![834362](https://github.com/user-attachments/assets/c8f3e25b-9460-4cb7-9acd-2198548ddd24)
![113560394993236](https://github.com/user-attachments/assets/733de6dd-727d-4550-b6a0-5b4666875cef)
![24643640](https://github.com/user-attachments/assets/938e3bda-5f0b-4e4d-8803-3d1af4c95ba9)
![1005068737248755752](https://github.com/user-attachments/assets/00e69899-9d92-47f0-9d5c-e4dc78a4f1f8)

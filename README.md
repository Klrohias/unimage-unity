# Unimage Unity
A simple library that can load and process images (in asynchronous) in Unity.  
Based on [Unimage Native](https://github.com/Klrohias/unimage-native).  


# Install
Open `Unity Editor > Window > Package Manager > Add from Git URL`:  
```
https://github.com/Klrohias/unimage-unity#0.1.1
```

# Usage
```csharp
using Unimage;
...

var processor = new UnimageProcessor();
processor.Load(uwr.downloadHandler.data);
processor.Resize(100, 100);
uiRawImage.texture = processor.GetTexture();

// or you can use async api

var processor = new UnimageProcessor();
await processor.LoadAsync(await File.ReadAllBytesAsync("..."));
await processor.ResizeAsync(100, 100);
uiRawImage.texture = await processor.GetTextureAsync();

```
# License
[MIT](LICENSE)

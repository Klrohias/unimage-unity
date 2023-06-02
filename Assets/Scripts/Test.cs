using System.Collections;
using Unimage;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private RawImage image;
    private void OnGUI()
    {
        if (GUILayout.Button("Test Load Async"))
        {
            StartCoroutine(TestAsync());
        }
        
        
        if (GUILayout.Button("Test Load"))
        {
            StartCoroutine(TestSync());
        }
    }

    private IEnumerator TestSync()
    {
        var request = UnityWebRequest.Get("file://C:/Users/qingy/Downloads/loser.jpeg");

        yield return request.SendWebRequest();
        
        var unimage = new UnimageProcessor();
        
        unimage.Load(request.downloadHandler.data);
        unimage.Resize(100, 100);
        unimage.Clip(25, 25, 50, 50);
        image.texture = unimage.GetTexture();
    }

    private IEnumerator TestAsync()
    {
        var request = UnityWebRequest.Get("file://C:/Users/qingy/Downloads/loser.jpeg");
        
        yield return request.SendWebRequest();
        
        TestLoadAsync(request);
    }

    private async void TestLoadAsync(UnityWebRequest request)
    {
        var unimage = new UnimageProcessor();
        await unimage.LoadAsync(request.downloadHandler.data);
        await unimage.ResizeAsync(100, 100);
        await unimage.ClipAsync(25, 25, 50, 50);
        image.texture = await unimage.GetTextureAsync();
    }
}

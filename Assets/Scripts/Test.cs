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
            StartCoroutine(TestLoad());
        }
        
        
        if (GUILayout.Button("Test Load"))
        {
            StartCoroutine(TestLoad2());
        }
    }

    private IEnumerator TestLoad2()
    {
        var request = UnityWebRequest.Get("file://C:/Users/qingy/Downloads/loser.jpeg");

        yield return request.SendWebRequest();
        
        var unimage = new UnimageHandle();
        
        unimage.Load(request.downloadHandler.data);
        
        Debug.Log(unimage.GetErrorMessage());
        unimage.Resize(100, 100);

        image.texture = unimage.ToTexture2D();
    }

    private IEnumerator TestLoad()
    {
        var request = UnityWebRequest.Get("file://C:/Users/qingy/Downloads/5120x2880.png");
        
        yield return request.SendWebRequest();
        
        TestLoadAsync(request);
    }

    private async void TestLoadAsync(UnityWebRequest request)
    {
        var unimage = new UnimageHandle();
        await unimage.LoadAsync(request.downloadHandler.data);
        image.texture = await unimage.ToTexture2DAsync();
    }
}

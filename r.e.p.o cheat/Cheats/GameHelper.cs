using UnityEngine;

public class GameHelper : MonoBehaviour
{
    public static new object FindObjectOfType(System.Type type)
    {
        return FindObjectOfType(type, true);
    }
}

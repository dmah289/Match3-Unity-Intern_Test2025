using UnityEngine;

namespace Utilities
{
    public static class ArrayExtension
    {
        public static void Shuffle<T>(this T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i+1);
                (array[i], array[j]) = (array[j], array[i]);
            } 
        }
    }
}
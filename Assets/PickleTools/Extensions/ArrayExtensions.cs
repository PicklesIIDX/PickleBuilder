using UnityEngine;

namespace PickleTools.Extensions.ArrayExtensions {
	public static class ArrayExtensions {

		public static T[] RemoveAt<T>(this T[] source, int index){
			T[] dest = new T[source.Length - 1];
			if(index > 0){
				System.Array.Copy(source, 0, dest, 0, index);
			}
			
			if(index < source.Length - 1){
				System.Array.Copy(source, index+1, dest, index, source.Length - index - 1);
			}
			return dest;
		}
		
		public static T[] InsertAt<T>(this T[] source, int index, T item){
			// split the array up
			T[] leftHandSide = new T[index];
			System.Array.Copy(source, 0, leftHandSide, 0, index);
			T[] rightHandSide = new T[source.Length - index];
			System.Array.Copy(source, index, rightHandSide, 0, source.Length - index);
			T[] dest = new T[source.Length + 1];
			System.Array.Copy(leftHandSide, 0, dest, 0, leftHandSide.Length);
			dest[leftHandSide.Length] = item;
			System.Array.Copy(rightHandSide, 0, dest, leftHandSide.Length + 1, rightHandSide.Length);
			
			return dest;
		}
		
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    abstract class ArrayHandler
    {
        /// <summary>
        /// Gets last index of the last non-null element from an array
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="array">array to process</param>
        /// <returns></returns>
        public static int GetIndexOfLastElement<T>(T[] array)
        {
            int result = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    result = i - 1;
                    break;
                }
                if (i == array.Length - 1)
                {
                    result = i;
                }
            }

            return result;
        }

        /// <summary>
        /// Inserts the a value in the array at the specified index
        /// </summary>
        /// <typeparam name="T">type of the array and of the value to insert</typeparam>
        /// <param name="index">index where the value should be inserted</param>
        /// <param name="array">array to insert the value in</param>
        /// <param name="value">element to be inserted</param>
        public static void InsertAt<T>(int index, T[] array, T value)
        {
            int lastIndex = ArrayHandler.GetIndexOfLastElement(array);
            if (index >= array.Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (lastIndex == array.GetUpperBound(0))
            {
                throw new Exception("--- Array is already full");
            }
            for (int i = lastIndex + 1; i > index; i--)
            {
                array[i] = array[i - 1];
            }
            array[index] = value;
        }

        /// <summary>
        /// Removes the element from the array at the specified index
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="index">index where the value should be removed from</param>
        /// <param name="array">array to process</param>
        public static void RemoveAt<T>(int index, T[] array)
        {
            int lastIndex = ArrayHandler.GetIndexOfLastElement(array);
            if (index >= array.Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (lastIndex == -1)
            {
                throw new Exception("--- Array is empty");
            }
            for (int i = index + 1; i <= lastIndex; i++)
            {
                array[i-1] = array[i];
            }
            array[lastIndex] = default(T);
        }
    }
}

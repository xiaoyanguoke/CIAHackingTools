/*
 * ArrayList.cpp
 *
 *  Created on: Aug 11, 2010
 *      Author: ???
 */

#include "CharArrayList.h"

template<class T> CharArrayList<T>::CharArrayList(int num)
{
	array = new T[arrayCapacity = num];
	currentLength = 0;
}

template<class T> CharArrayList<T>::~CharArrayList()
{
	delete [] array;
}

template<class T> void CharArrayList<T>::addChar(const T num)
{
	if( currentLength == arrayCapacity )
		increaseCapacity(10);
	array[currentLength++] = num;
}

template<class T> T CharArrayList<T>::getChar(int n)
{
	if( n < currentLength )
		return array[n];
	else
		return -1;
}

template<class T> void CharArrayList<T>::remove(int index)
{
	for(int i = index; i < currentLength - 1; i++)
		array[i] = array[i + 1];
	currentLength--;
}

template<class T> int CharArrayList<T>::getSize()
{
	return currentLength;
}
template<class T> int CharArrayList<T>::getCapacity()
{
	return arrayCapacity;
}
template<class T> T* CharArrayList<T>::getArray()
{
	if( currentLength == arrayCapacity )
		increaseCapacity(10);
	array[currentLength] = '\0';
	return array;
}

template<class T> void CharArrayList<T>::increaseCapacity(int val)
{
	T* tempArray = new T[arrayCapacity+=val];
	memcpy( tempArray, array, sizeof(T) * currentLength);
	delete[] array;
	array = tempArray;
}

template<class T> void CharArrayList<T>::erase(int num)
{
	delete [] array;
	array = new T[arrayCapacity = num];
	currentLength = 0;
}

/*
 * ArrayList.h
 *
 *  Created on: Aug 11, 2010
 *      Author: ???
 */

#pragma once
#include <string.h>

template<class T> class CharArrayList
{
private:
	T* array;
	int arrayCapacity, currentLength;
	void increaseCapacity(int);
public:
	CharArrayList(int num = 10);
	virtual ~CharArrayList();

	void addChar(const T);
	T getChar(int);
	void remove(int);
	int getSize();
	int getCapacity();
	T* getArray();
	void erase(int num = 10);
};

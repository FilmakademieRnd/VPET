/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/
#include "SensorLib.h"

#using <Windows.winmd>
using namespace Windows::Devices::Sensors;
#using <Platform.winmd>
using namespace Platform;

extern "C"
{
	Gyrometer^ wsaGyro;
	Compass^ wsaCompass;
	OrientationSensor^ wsaOrientationSensor;
	Inclinometer^ wsaInclinometer;

	GyrometerReading^ wsaGyroData;
	CompassReading^ wsaCompassData;
	OrientationSensorReading^ wsaOrientationSensorData;
	InclinometerReading^ wsaInclinometerData;

	float gyroDataRAW[3];
	float compassDataRAW[1];
	float orientationDataRAW[4];
	float inclinationDataRAW[3];

	bool initalized = false;

	[Platform::MTAThread]
	void initalizeSensorReading()
	{
		wsaGyro = Gyrometer::GetDefault();
		wsaCompass = Compass::GetDefault();
		wsaOrientationSensor = OrientationSensor::GetDefault();
		wsaInclinometer = Inclinometer::GetDefault();
		initalized = true;
	}

	[Platform::MTAThread]
	const void* getGyroData()
	{
		if (wsaGyro)
		{
			wsaGyroData = wsaGyro->GetCurrentReading();
		}
		if (wsaGyro && wsaGyroData && initalized)
		{
			gyroDataRAW[0] = (float)wsaGyroData->AngularVelocityX;
			gyroDataRAW[1] = (float)wsaGyroData->AngularVelocityY;
			gyroDataRAW[2] = (float)wsaGyroData->AngularVelocityZ;
		}
		else
		{
			gyroDataRAW[0] = 1;
			gyroDataRAW[1] = 2;
			gyroDataRAW[2] = 3;
		}
		return gyroDataRAW;
	}

	const void* getCompassData()
	{
		if (wsaCompass)
		{
			wsaCompassData = wsaCompass->GetCurrentReading();
		}
		if (wsaCompass && wsaCompassData && initalized)
		{
			compassDataRAW[0] = (float)wsaCompassData->HeadingTrueNorth->Value;
		}
		else
		{
			compassDataRAW[0] = 1;
		}
		return compassDataRAW;
	}

	const void* getOrientationSensorData()
	{
		if (wsaOrientationSensor)
		{
			wsaOrientationSensorData = wsaOrientationSensor->GetCurrentReading();
		}
		if (wsaOrientationSensor && wsaOrientationSensorData && initalized)
		{
			orientationDataRAW[0] = wsaOrientationSensorData->Quaternion->X;
			orientationDataRAW[1] = wsaOrientationSensorData->Quaternion->Y;
			orientationDataRAW[2] = wsaOrientationSensorData->Quaternion->Z;
			orientationDataRAW[3] = wsaOrientationSensorData->Quaternion->W;
		}
		else
		{
			orientationDataRAW[0] = 1;
			orientationDataRAW[1] = 2;
			orientationDataRAW[2] = 3;
			orientationDataRAW[3] = 4;
		}
		return orientationDataRAW;
	}

	const void* getInclinometerData()
	{
		if (wsaInclinometer)
		{
			wsaInclinometerData = wsaInclinometer->GetCurrentReading();
		}
		if (wsaInclinometer && wsaInclinometerData && initalized)
		{
			inclinationDataRAW[0] = wsaInclinometerData->PitchDegrees;
			inclinationDataRAW[1] = wsaInclinometerData->RollDegrees;
			inclinationDataRAW[2] = wsaInclinometerData->YawDegrees;
		}
		else
		{
			inclinationDataRAW[0] = 1;
			inclinationDataRAW[1] = 2;
			inclinationDataRAW[2] = 3;
		}
		return inclinationDataRAW;
	}
}
// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectCamera.h"

// Message parsing pre-declarations
void UpdateFov(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateAspect(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateNearClipPlane(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateFarClipPlane(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateFocalDistance(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateAperture(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateSensorSize(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectCamera::BeginPlay()
{
	Super::BeginPlay();

	kCam = Cast<ACameraActor>(thisActor);
	kCamComp = kCam->GetCameraComponent();
	kCineCam = Cast<ACineCameraActor>(thisActor);
	if(kCineCam)
		kCineCamComp = kCineCam->GetCineCameraComponent();
	if (kCamComp)
	{
		float aspect = kCamComp->AspectRatio;

		float fov = 2 * atan(tan(kCamComp->FieldOfView / 2.0 * DEG2RAD) / aspect) / DEG2RAD;
		new Parameter<float>(fov, thisActor, "fov", &UpdateFov, this);
		fovBuffer = fov;

		new Parameter<float>(aspect, thisActor, "aspectRatio", &UpdateAspect, this);
		aspectBuffer = aspect;

		float nearVPET = 0.001;
		new Parameter<float>(nearVPET, thisActor, "nearClipPlane", &UpdateNearClipPlane, this);
		nearBuffer = nearVPET;

		float farVPET = 100;
		new Parameter<float>(farVPET, thisActor, "farClipPlane", &UpdateFarClipPlane, this);
		farBuffer = farVPET;

		// Default values
		float focDist = 1;
		float aperture = 2.8;
		FVector2D sensor(36, 24);
		
		// Cine camera valupes
		if (kCineCamComp)
		{
			focDist = kCineCamComp->CurrentFocusDistance;
			aperture = kCineCamComp->CurrentAperture;
			sensor.X = kCineCamComp->Filmback.SensorWidth;
			sensor.Y = kCineCamComp->Filmback.SensorHeight;
		}

		new Parameter<float>(focDist, thisActor, "focalDistance", &UpdateFocalDistance, this);
		focDistBuffer = focDist;

		new Parameter<float>(aperture, thisActor, "aperture", &UpdateAperture, this);
		apertureBuffer = aperture;

		new Parameter<FVector2D>(sensor, thisActor, "sensorSize", &UpdateSensorSize, this);
		sensorBuffer = sensor;
	
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectCamera::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (kCamComp)
	{
		float aspect = kCamComp->AspectRatio; // default: 2
		float fov = 2 * atan(tan(kCamComp->FieldOfView / 2.0 * DEG2RAD) / aspect) / DEG2RAD; // default: 90
		float nearVPET = 0.001; // default: 0.001
		float farVPET = 100; // default: 100
		float focDist = 1; // default: 1
		float aperture = 2.8; // default: 2.8
		FVector2D sensor(36, 24); // default: 36, 24
		if (kCineCamComp)
		{
			focDist = kCineCamComp->CurrentFocusDistance;
			aperture = kCineCamComp->CurrentAperture;
			sensor.X = kCineCamComp->Filmback.SensorWidth;
			sensor.Y = kCineCamComp->Filmback.SensorHeight;
		}

		if (fov != fovBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("FOV CHANGE: %f"), fov);
			fovBuffer = fov;
			EncodeParameterFovMessage();
		}
		if (aspect != aspectBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("ASPECT CHANGE: %f"), aspect);
			aspectBuffer = aspect;
			EncodeParameterAspectMessage();
		}
		if (nearVPET != nearBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("NEAR CHANGE: %f"), nearVPET);
			nearBuffer = nearVPET;
			EncodeParameterNearMessage();
		}
		if (farVPET != farBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("FAR CHANGE: %f"), farVPET);
			farBuffer = farVPET;
			EncodeParameterFarMessage();
		}
		if (focDist != focDistBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("FOCDIST CHANGE: %f"), focDist);
			focDistBuffer = focDist;
			EncodeParameterFocDistMessage();
		}
		if (aperture != apertureBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("APERTURE CHANGE: %f"), aperture);
			apertureBuffer = aperture;
			EncodeParameterApertureMessage();
		}
		if (sensor != sensorBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("SENSOR CHANGE: %f %f"), sensor.X, sensor.Y);
			sensorBuffer = sensor;
			EncodeParameterSensorMessage();
		}
	}
}

// Prepare a message for fov change
void USceneObjectCamera::EncodeParameterFovMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - lit
	shortVal = 4;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = fovBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for aspect change
void USceneObjectCamera::EncodeParameterAspectMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - lit
	shortVal = 4;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = aspectBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for near change
void USceneObjectCamera::EncodeParameterNearMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - lit
	shortVal = 4;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = nearBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for far change
void USceneObjectCamera::EncodeParameterFarMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - lit
	shortVal = 4;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = farBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for focDist change
void USceneObjectCamera::EncodeParameterFocDistMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - lit
	shortVal = 4;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = focDistBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for fov change
void USceneObjectCamera::EncodeParameterApertureMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - lit
	shortVal = 4;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = apertureBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for sensor change
void USceneObjectCamera::EncodeParameterSensorMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + vector
	responseLength = 8 + 3 * sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - loc
	shortVal = 0;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 4 for vec2
	intVal = 4;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = sensorBuffer[0];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = sensorBuffer[1];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Parses a message for fov change
void UpdateFov(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	ACineCameraActor* kCineCam = Cast<ACineCameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type FOV: %f"), lK);

		float aspect = kCam->GetCameraComponent()->AspectRatio;
		float fov = 2.0 * atan(tan(lK / 2.0 * DEG2RAD) * aspect) / DEG2RAD;
		kCam->GetCameraComponent()->FieldOfView = fov;

		if (kCineCam != NULL)
		{
			kCineCam->GetCineCameraComponent()->SetFieldOfView(fov);
		}
	}
}

// Parses a message for aspect ratio change - does not work properly with CineCameras
void UpdateAspect(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type ASPECT RATIO: %f"), lK);

		kCam->GetCameraComponent()->AspectRatio = lK;
	}
}

// Parses a message for near clip change
void UpdateNearClipPlane(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type NEAR CLIP PLANE: %f (not used)"), lK);
	}
}

// Parses a message for far clip change
void UpdateFarClipPlane(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type FAR CLIP PLANE: %f (not used)"), lK);
	}
}

// Parses a message for focal distance change
void UpdateFocalDistance(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	ACineCameraActor* kCineCam = Cast<ACineCameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);

		if (kCineCam == NULL)
		{
			UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type FOCAL DISTANCE: %f (camera does not support it)"), lK);
		}
		else
		{
			UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type FOCAL DISTANCE: %f"), lK);
			
			kCineCam->GetCineCameraComponent()->FocusSettings.ManualFocusDistance = lK;
		}
	}
}

// Parses a message for aperture change
void UpdateAperture(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	ACineCameraActor* kCineCam = Cast<ACineCameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);

		if (kCineCam == NULL)
		{
			UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type APERTURE: %f (camera does not support it)"), lK);
		}
		else
		{
			UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type APERTURE: %f"), lK);

			kCineCam->GetCineCameraComponent()->CurrentAperture = lK;
		}
	}
}

// Parses a message for sensor size change
void UpdateSensorSize(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	ACineCameraActor* kCineCam = Cast<ACineCameraActor>(actor);
	if (kCam)
	{
		float lX = *reinterpret_cast<float*>(&kMsg[8]);
		float lY = *reinterpret_cast<float*>(&kMsg[12]);

		if (kCineCam == NULL)
		{
			UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SENSOR SIZE: %f %f (camera does not support it)"), lX, lY);
		}
		else
		{
			UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SENSOR SIZE: %f %f"), lX, lY);

			kCineCam->GetCineCameraComponent()->Filmback.SensorWidth = lX;
			kCineCam->GetCineCameraComponent()->Filmback.SensorHeight = lY;
		}
	}
}
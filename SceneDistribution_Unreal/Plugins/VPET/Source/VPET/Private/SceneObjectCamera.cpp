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
	if (kCamComp)
	{
		float aspect = kCamComp->AspectRatio;

		float fov = atan(tan(kCamComp->FieldOfView / 2) * aspect);
		new Parameter<float>(fov, thisActor, "fov", &UpdateFov, this);
		fovBuffer = fov;

		new Parameter<float>(aspect, thisActor, "aspectRatio", &UpdateAspect, this);
		aspectBuffer = aspect;

		float near = 0.001;
		new Parameter<float>(near, thisActor, "nearClipPlane", &UpdateNearClipPlane, this);
		nearBuffer = near;

		float far = 100;
		new Parameter<float>(far, thisActor, "farClipPlane", &UpdateFarClipPlane, this);
		farBuffer = far;

		float focDist = 1;
		new Parameter<float>(focDist, thisActor, "focalDistance", &UpdateFocalDistance, this);
		focDistBuffer = focDist;

		float aperture = 2.8;
		new Parameter<float>(aperture, thisActor, "aperture", &UpdateAperture, this);
		apertureBuffer = aperture;

		FVector2D sensor(36, 24);
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
		float fov = atan(tan(kCamComp->FieldOfView / 2) * aspect); // default: 90
		float near = 0.001; // default: 0.001
		float far = 100; // default: 100
		float focDist = 1; // default: 1
		float aperture = 2.8; // default: 2.8
		FVector2D sensor(36, 24); // default: 36, 24

		if (fov != fovBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("FOV CHANGE"));
			fovBuffer = fov;
			EncodeParameterFovMessage();
		}
		if (aspect != aspectBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("ASPECT CHANGE"));
			aspectBuffer = aspect;
			EncodeParameterAspectMessage();
		}
		if (near != nearBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("NEAR CHANGE"));
			nearBuffer = near;
			EncodeParameterNearMessage();
		}
		if (far != farBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("FAR CHANGE"));
			farBuffer = far;
			EncodeParameterFarMessage();
		}
		if (focDist != focDistBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("FOCDIST CHANGE"));
			focDistBuffer = focDist;
			EncodeParameterFocDistMessage();
		}
		if (aperture != apertureBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("APERTURE CHANGE"));
			apertureBuffer = aperture;
			EncodeParameterApertureMessage();
		}
		if (sensor != sensorBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("SENSOR CHANGE"));
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
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type FOV: %f"), lK);

		float aspect = kCam->GetCameraComponent()->AspectRatio;
		kCam->GetCameraComponent()->FieldOfView = 2.0 * atan(tan(lK / 2.0 * DEG2RAD) * aspect) / DEG2RAD;
	}
}

// Parses a message for aspect ratio change
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
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type FOCAL DISTANCE: %f (not used)"), lK);
	}
}

// Parses a message for aperture change
void UpdateAperture(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type APERTURE: %f (not used)"), lK);
	}
}

// Parses a message for sensor size change
void UpdateSensorSize(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	if (kCam)
	{
		float lX = *reinterpret_cast<float*>(&kMsg[8]);
		float lY = *reinterpret_cast<float*>(&kMsg[12]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SENSOR SIZE: %f %f"), lX, lY);
	}
}
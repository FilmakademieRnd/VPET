// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectLight.h"

// Message parsing pre-declarations
void UpdateColor(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateIntensity(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectLight::BeginPlay()
{
	Super::BeginPlay();

	kLit = Cast<ALight>(thisActor);
	if (kLit)
	{
		FVector col = kLit->GetLightColor();
		Parameter<FVector>* color = new Parameter<FVector>(col, thisActor, "color", &UpdateColor, this);
		colBuffer = col;

		float lit = kLit->GetBrightness() * lightFactor;
		Parameter<float>* intensity = new Parameter<float>(lit, thisActor, "intensity", &UpdateIntensity, this);
		litBuffer = lit;
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (kLit)
	{
		FVector col = kLit->GetLightColor();
		float lit = kLit->GetBrightness() * lightFactor;

		if (col != colBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("COL CHANGE"));
			colBuffer = col;
			EncodeParameterColMessage();
		}
		if (lit != litBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("LIT CHANGE"));
			litBuffer = lit;
			EncodeParameterLitMessage();
		}
	}
}

// Prepare a message for color change
void USceneObjectLight::EncodeParameterColMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + vector
	responseLength = 8 + 4 * sizeof(float);
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
	// Parameter ID - col
	shortVal = 3;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 6 for vec4
	intVal = 6;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = colBuffer[0];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = colBuffer[1];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = colBuffer[2];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = 1; // alpha
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for intensity change
void USceneObjectLight::EncodeParameterLitMessage()
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
	float msgVal = litBuffer / lightFactor;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Parses a message for color change
void UpdateColor(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type COLOR"));
	ALight* kLit = Cast<ALight>(actor);
	if (kLit)
	{
		float lR = *reinterpret_cast<float*>(&kMsg[8]);
		float lG = *reinterpret_cast<float*>(&kMsg[12]);
		float lB = *reinterpret_cast<float*>(&kMsg[16]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type COLOR: %f %f %f"), lR, lG, lB);

		kLit->SetLightColor(FLinearColor(lR, lG, lB));
	}
}

// Parses a message for intensity change
void UpdateIntensity(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type INTENSITY"));
	ALight* kLit = Cast<ALight>(actor);
	if (kLit)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type INTENSITY: %f"), lK);

		float lightFactor = 0.2;
		kLit->SetBrightness(lK / lightFactor);
	}
}
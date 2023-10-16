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

		float focDist = 1;
		float aperture = 2.8;
		FVector2D sensor(36, 24);
		float farVPET = 100;
		float nearVPET = 0.001;

		// Cine camera valupes
		if (kCineCamComp)
		{
			focDist = kCineCamComp->CurrentFocusDistance;
			aperture = kCineCamComp->CurrentAperture;
			sensor.X = kCineCamComp->Filmback.SensorWidth;
			sensor.Y = kCineCamComp->Filmback.SensorHeight;
		}
		
		FOV_Vpet_Param = new Parameter<float>(fov, thisActor, "fov", &UpdateFov, this);
		Aspect_Vpet_Param = new Parameter<float>(aspect, thisActor, "aspectRatio", &UpdateAspect, this);
		Near_Vpet_Param = new Parameter<float>(nearVPET, thisActor, "nearClipPlane", &UpdateNearClipPlane, this);
		Far_Vpet_Param = new Parameter<float>(farVPET, thisActor, "farClipPlane", &UpdateFarClipPlane, this);
		FocDist_Vpet_Param = new Parameter<float>(focDist, thisActor, "focalDistance", &UpdateFocalDistance, this);
		Aperture_Vpet_Param = new Parameter<float>(aperture, thisActor, "aperture", &UpdateAperture, this);
		Sensor_Vpet_Param =new Parameter<FVector2D>(sensor, thisActor, "sensorSize", &UpdateSensorSize, this);
		
	
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectCamera::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (_lock)
		return;
	
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

		if (fov != FOV_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("FOV CHANGE: %f"), fov);
			ParameterObject_HasChanged.Broadcast(FOV_Vpet_Param);
			FOV_Vpet_Param->setValue(fov);
			
		}
		if (aspect != Aspect_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("ASPECT CHANGE: %f"), aspect);
			ParameterObject_HasChanged.Broadcast(Aspect_Vpet_Param);
			Aspect_Vpet_Param->setValue(aspect);
		}
		if (nearVPET != Near_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("NEAR CHANGE: %f"), nearVPET);
			ParameterObject_HasChanged.Broadcast(Near_Vpet_Param);
			Near_Vpet_Param->setValue(nearVPET);
		}
		if (farVPET != Far_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("FAR CHANGE: %f"), farVPET);
			ParameterObject_HasChanged.Broadcast(Far_Vpet_Param);
			Far_Vpet_Param->setValue(farVPET);
		}
		if (focDist != FocDist_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("FOCDIST CHANGE: %f"), focDist);
			ParameterObject_HasChanged.Broadcast(Far_Vpet_Param);
			FocDist_Vpet_Param->setValue(focDist);
		}
		if (aperture != Aperture_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("APERTURE CHANGE: %f"), aperture);
			ParameterObject_HasChanged.Broadcast(Aperture_Vpet_Param);
			Aperture_Vpet_Param->setValue(aperture);
		}
		if (sensor != Sensor_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("SENSOR CHANGE: %f %f"), sensor.X, sensor.Y);
			ParameterObject_HasChanged.Broadcast(Sensor_Vpet_Param);
			Sensor_Vpet_Param->setValue(sensor);
		}
	}
}

// Parses a message for fov change
void UpdateFov(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	ACineCameraActor* kCineCam = Cast<ACineCameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[0]);
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
		float lK = *reinterpret_cast<float*>(&kMsg[0]);
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
		float lK = *reinterpret_cast<float*>(&kMsg[0]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type NEAR CLIP PLANE: %f (not used)"), lK);
	}
}

// Parses a message for far clip change
void UpdateFarClipPlane(std::vector<uint8_t> kMsg, AActor* actor)
{
	ACameraActor* kCam = Cast<ACameraActor>(actor);
	if (kCam)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[0]);
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
		float lK = *reinterpret_cast<float*>(&kMsg[0]);

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
		float lK = *reinterpret_cast<float*>(&kMsg[0]);

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
		float lX = *reinterpret_cast<float*>(&kMsg[0]);
		float lY = *reinterpret_cast<float*>(&kMsg[4]);

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
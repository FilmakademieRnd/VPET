// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <zmq.hpp>
#include "Async/AsyncWork.h"
#include "VPETModule.h"

// Development output log macro
#define DOL(logType, logVerbosity, logString, ...) \
if (logType) { \
UE_LOG(LogTemp, logVerbosity, TEXT(logString), ##__VA_ARGS__); \
}

// Development on-screen debug message
#define OSD(debugColor, debugString, ...) if(GEngine && VerboseDisplay) GEngine->AddOnScreenDebugMessage(-1, 5.0f, debugColor, FString::Printf(TEXT(debugString), __VA_ARGS__));


// Update receiver thread
class UpdateReceiverThread : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<UpdateReceiverThread>;
public:
	zmq::socket_t* socket;
	std::vector<std::vector<uint8_t>>* msgQ;
	bool doLog;
	uint8_t cID;
	AVPETModule* manager;

	enum class MessageType
	{
		PARAMETERUPDATE, LOCK, // node
		SYNC, PING, RESENDUPDATE, // sync
		UNDOREDOADD, RESETOBJECT // undo redo
	};

	UpdateReceiverThread(zmq::socket_t* pSocket, std::vector<std::vector<uint8_t>>* pQueue, uint8_t m_ID, bool pLog, AVPETModule* pMod) : socket(pSocket), msgQ(pQueue), cID(m_ID), doLog(pLog), manager(pMod) { }

	void DoWork();

	FORCEINLINE TStatId GetStatId() const
	{
		RETURN_QUICK_DECLARE_CYCLE_STAT(FGenericTask, STATGROUP_TaskGraphTasks);
	}

};
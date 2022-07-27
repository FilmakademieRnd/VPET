// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <zmq.hpp>
#include "Async/AsyncWork.h"

// Development output log macro
#define DOL(logType, logVerbosity, logString, ...) if(logType) UE_LOG(LogTemp, logVerbosity, TEXT(logString), __VA_ARGS__);

// Development on-screen debug message
#define OSD(debugColor, debugString, ...) if(GEngine && VerboseDisplay) GEngine->AddOnScreenDebugMessage(-1, 5.0f, debugColor, FString::Printf(TEXT(debugString), __VA_ARGS__));


// Update Sender thread
class UpdateSenderThread : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<UpdateSenderThread>;
public:
	zmq::socket_t* socket;
	//std::vector<uint8_t*>* msgQ;
	std::vector<std::vector<uint8_t>>* msgQ;
	std::vector<char*>* msgData;
	std::vector<int>* msgLen;
	bool doLog;
	//bool* doRun;
	uint8_t cID;

	enum class MessageType
	{
		PARAMETERUPDATE, LOCK, // node
		SYNC, PING, RESENDUPDATE, // sync
		UNDOREDOADD, RESETOBJECT // undo redo
	};

	//ThreadSyncDev(zmq::socket_t* pSocket, std::vector<uint8_t*>* pQueue, bool pLog) : socket(pSocket), msgQ(pQueue), doLog(pLog) { }
	//ThreadVPET2Recv(zmq::socket_t* pSocket, std::vector<std::vector<uint8_t>>* pQueue, bool pLog) : socket(pSocket), msgQ(pQueue), doLog(pLog) { }
	//ThreadVPET2Send(zmq::socket_t* pSocket, std::vector<std::vector<uint8_t>>* pQueue, uint8_t m_ID, bool pLog, bool* pRun) : socket(pSocket), msgQ(pQueue), cID(m_ID), doLog(pLog), doRun(pRun) { }
	UpdateSenderThread(zmq::socket_t* pSocket, std::vector<std::vector<uint8_t>>* pQueue, uint8_t m_ID, bool pLog, std::vector<char*>* pData, std::vector<int>* pLen) : socket(pSocket), msgQ(pQueue), cID(m_ID), doLog(pLog), msgData(pData), msgLen(pLen) { }

	void DoWork();

	FORCEINLINE TStatId GetStatId() const
	{
		RETURN_QUICK_DECLARE_CYCLE_STAT(FGenericTask, STATGROUP_TaskGraphTasks);
	}

};
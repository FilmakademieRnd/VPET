
#if PLATFORM_WINDOWS && !defined(WINDOWS_PLATFORM_TYPES_GUARD)
	#include <Windows/AllowWindowsPlatformTypes.h>
	#include "zmq_impl.hpp"
	#include <Windows/HideWindowsPlatformTypes.h>
#else
	#include "zmq_impl.hpp"
#endif


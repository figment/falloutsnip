// This is the main DLL file.

#include "stdafx.h"

#include <stdlib.h>
#include <string.h>
#include <limits.h>
#include <time.h>
#include <stdarg.h>
#include <varargs.h>
#include <vcclr.h>

#include "ZLibMC.h"

namespace ZLibC {
#include "..\zlib-1.2.8\zlib.h"
}

using namespace System;
using namespace System::Runtime::InteropServices;

inline void ManagedStringToBuffer(String^ value, char* buffer, int len, char fill)
{
	pin_ptr<const wchar_t> pcval = PtrToStringChars(value);
	size_t pcvallen = wcslen(pcval);
	if (pcvallen > len-1)
		pcvallen = len-1;
	size_t n;
	wcstombs_s(&n, buffer, len, pcval, pcvallen);
	memset(buffer+pcvallen, fill, len-pcvallen);
}

inline String^ BufferToManagedString(const char* buffer)
{
	size_t pcvallen = strlen(buffer);
	return gcnew String( buffer, 0, pcvallen, System::Text::Encoding::ASCII);
}

namespace DotZLib 
{
	using namespace System;

	public ref class ZLibException : ApplicationException
	{
	public:
		ZLibException(int errorCode) : ApplicationException(String::Format("ZLib error {0}", errorCode)) { }
		ZLibException(int errorCode, String^ msg) : ApplicationException(String::Format("ZLib error {0} {1}", errorCode, msg)) {}
	};

	public ref class ZLib 
	{
	public:
		static void Initialize() {}

		static String^ Version() {
			const char * p = ZLibC::zlibVersion();
			return BufferToManagedString(p); 
		}

		static array<unsigned char>^ Compress(array<unsigned char>^ data, int offset, int length, int level)
		{
			const int CHUNK = 16384;
			int ret, flush, n, left;
			unsigned int have;
			ZLibC::z_stream strm;
			unsigned char in[CHUNK];
			unsigned char out[CHUNK];
			array<unsigned char>^ gcout = gcnew array<unsigned char>(CHUNK);

			/* allocate deflate state */
			strm.zalloc = Z_NULL;
			strm.zfree = Z_NULL;
			strm.opaque = Z_NULL;
			ret = ZLibC::deflateInit_(&strm, level, ZLibC::zlibVersion(), (int)sizeof(ZLibC::z_stream));
			if (ret != Z_OK)
				throw gcnew ZLibException(ret, "Could not initialize deflater") ;



			System::IO::MemoryStream^ outstream = gcnew System::IO::MemoryStream(length);
			pin_ptr<unsigned char> p = &data[offset];
			pin_ptr<unsigned char> pout = &gcout[0];
			left = length;
			do {
				if (left == 0) break;
				n = CHUNK < left ? CHUNK : left;
				memcpy(in, p, n); p += n;
				strm.avail_in = n;
				left -= n;

				flush = left == 0 ? Z_FINISH : Z_NO_FLUSH;
				strm.next_in = in;

				/* run deflate() on input until output buffer not full, finish
				compression if all of source has been read in */
				do {
					strm.avail_out = CHUNK;
					strm.next_out = out;
					ret = ZLibC::deflate(&strm, flush);    /* no bad return value */
					if (ret == Z_STREAM_ERROR)
						throw gcnew ZLibException(ret, "Could not deflate") ;
					have = CHUNK - strm.avail_out;
					memcpy(pout, out, have);
					outstream->Write(gcout, 0, have);
				} while (strm.avail_out == 0);
				/* done when last data in clfile processed */
			} while (flush != Z_FINISH);
			/* clean up and return */
			(void)deflateEnd(&strm);

			return outstream->ToArray();
		}

		static array<unsigned char>^ Decompress(array<unsigned char>^ data, int offset, int length, int compSize, [Out] int% level)
		{
			// We know size so just allocate the output array.  If we dont know size use the other method
			if (compSize <= 0) return Decompress(data, offset, length, level);

			const int CHUNK = 16384;
			int ret, flush, n, left;
			size_t have;
			ZLibC::z_stream strm;
			unsigned char in[CHUNK];
			unsigned char out[CHUNK];
			array<unsigned char>^ gcout = gcnew array<unsigned char>(compSize);


			/* allocate deflate state */
			strm.zalloc = Z_NULL;
			strm.zfree = Z_NULL;
			strm.opaque = Z_NULL;
			strm.avail_in = 0;
			strm.next_in = Z_NULL;
			ret = ZLibC::inflateInit_(&strm, ZLibC::zlibVersion(), (int)sizeof(ZLibC::z_stream));
			if (ret != Z_OK)
				throw gcnew ZLibException(ret, "Could not initialize deflater") ;

			level = RetrieveCompressionLevel(data, offset, length);

			pin_ptr<unsigned char> p = &data[offset];
			pin_ptr<unsigned char> pout = &gcout[0];
			int poutidx = 0;
			left = length;
			do {
				if (left == 0) break;
				n = CHUNK < left ? CHUNK : left;
				memcpy(in, p, n); p += n;
				strm.avail_in = n;
				left -= n;

				flush = left == 0 ? Z_FINISH : Z_NO_FLUSH;
				strm.next_in = in;

				/* run deflate() on input until output buffer not full, finish
				compression if all of source has been read in */
				do {
					strm.avail_out = CHUNK;
					strm.next_out = out;
					ret = ZLibC::inflate(&strm, Z_NO_FLUSH);
					switch (ret) {
					case Z_NEED_DICT:
						ret = Z_DATA_ERROR;     /* and fall through */
					case Z_DATA_ERROR:
					case Z_MEM_ERROR:
					case Z_STREAM_ERROR:
						(void)inflateEnd(&strm);
						throw gcnew ZLibException(ret, "Could not inflate") ;
					}
					have = CHUNK - strm.avail_out;
					memcpy(pout, out, have); pout += have;
				} while (strm.avail_out == 0);
				/* done when last data in clfile processed */
			} while (ret != Z_STREAM_END);
			/* clean up and return */
			(void)inflateEnd(&strm);
			if (ret != Z_STREAM_END) throw gcnew ZLibException(Z_DATA_ERROR, "Could not inflate") ;
			return gcout;
		}
		// Decompress
		//   Version with unknown output length
		static array<unsigned char>^ Decompress(array<unsigned char>^ data, int offset, int length,  [Out] int% level)
		{
			const int CHUNK = 16384;
			int ret, flush, n, left;
			size_t have;
			ZLibC::z_stream strm;
			unsigned char in[CHUNK];
			unsigned char out[CHUNK];
			array<unsigned char>^ gcout = gcnew array<unsigned char>(CHUNK);


			/* allocate deflate state */
			strm.zalloc = Z_NULL;
			strm.zfree = Z_NULL;
			strm.opaque = Z_NULL;
			strm.avail_in = 0;
			strm.next_in = Z_NULL;
			ret = ZLibC::inflateInit_(&strm, ZLibC::zlibVersion(), (int)sizeof(ZLibC::z_stream));
			if (ret != Z_OK)
				throw gcnew ZLibException(ret, "Could not initialize deflater") ;

			level = RetrieveCompressionLevel(data, offset, length);

			System::IO::MemoryStream^ outstream = gcnew System::IO::MemoryStream(length);
			pin_ptr<unsigned char> p = &data[offset];
			pin_ptr<unsigned char> pout = &gcout[0];
			int inidx = 0;
			left = length;
			do {
				if (left == 0) break;
				n = CHUNK < left ? CHUNK : left;
				memcpy(in, p, n); p += n;
				strm.avail_in = n;
				left -= n;

				flush = left == 0 ? Z_FINISH : Z_NO_FLUSH;
				strm.next_in = in;

				/* run deflate() on input until output buffer not full, finish
				compression if all of source has been read in */
				do {
					strm.avail_out = CHUNK;
					strm.next_out = out;
					ret = ZLibC::inflate(&strm, Z_NO_FLUSH);
					switch (ret) {
					case Z_NEED_DICT:
						ret = Z_DATA_ERROR;     /* and fall through */
					case Z_DATA_ERROR:
					case Z_MEM_ERROR:
					case Z_STREAM_ERROR:
						(void)inflateEnd(&strm);
						throw gcnew ZLibException(ret, "Could not inflate") ;
					}

					have = CHUNK - strm.avail_out;
					memcpy(pout, out, have);
					outstream->Write(gcout, 0, have);
				} while (strm.avail_out == 0);
				/* done when last data in clfile processed */
			} while (ret != Z_STREAM_END);
			/* clean up and return */
			(void)inflateEnd(&strm);
			if (ret != Z_STREAM_END) throw gcnew ZLibException(Z_DATA_ERROR, "Could not inflate") ;
			return outstream->ToArray();
		}

		static void Close() {}

		private:
		static int RetrieveCompressionLevel(array<unsigned char>^ data, int offset, int length)
        {
			if (length < 2) return 0;

            unsigned char cmf = data[offset+0];
            unsigned char flg = data[offset+1];
            flg = (flg & 0xC0); // Mask : 11000000 = 192 = 0xC0
            flg = flg >> 6;
            switch (flg)
            {
                case 0: return 0; 
                case 1: return 1; 
                case 2: return -1; 
                case 3: return 9; 
                default: return 9; 
            }
        }

	};

#ifdef ASDF
	private enum class FlushTypes
	{
		// Enumerators
		Block = 5,
		Finish = 4,
		Full = 3,
		None = 0,
		Partial = 1,
		Sync = 2,
	};

	public enum class CompressLevel
	{
		// Enumerators
		Best = 9,
		Default = -1,
		Fastest = 1,
		None = 0,
	};

	public delegate void DataAvailableHandler(array<unsigned char>^ data, int startIndex, int count);

	[StructLayout(LayoutKind::Sequential, Pack=4)]
	private value class ZStream
	{
		// Fields
	public:
		unsigned int adler;
		unsigned int avail_in;
		unsigned int avail_out;
		String^ msg;
		IntPtr next_in;
		IntPtr next_out;
		unsigned int total_in;
		unsigned int total_out;
	private:
		initonly unsigned int opaque;
		initonly unsigned int reserved;
		initonly unsigned int state;
		initonly unsigned int zalloc;
		initonly unsigned int zfree;
		initonly int data_type;
	};

	public ref class Info
	{
	public:
		Info::Info()
		{
			_flags = Info::zlibCompileFlags();
		}
		// Methods
	private:
		static int bitSize(unsigned int bits)
		{
			switch (bits)
			{
			case 0: return 16;
			case 1: return 32;
			case 2: return 64;
			}
			return -1;
		}
	private:
		unsigned int zlibCompileFlags()
		{
			return zlibCompileFlags();
		}
	private:
		static String^ zlibVersion()
		{
			const char * p = ZLibC::zlibVersion();
			return BufferToManagedString(p); 
		}
		// Fields
	private:
		initonly unsigned int _flags;
		// Properties
		property Info^ HasDebugInfo;
		property Info^ SizeOfOffset;
		property Info^ SizeOfPointer;
		property Info^ SizeOfUInt;
		property Info^ SizeOfULong;
		property Info^ UsesAssemblyCode;
		property Info^ Version;
	};

	public interface class Codec
	{
		// Methods
		void Add(array<unsigned char>^ data) abstract;
		void Add(array<unsigned char>^ data, int offset, int count) abstract;
		void Finish() abstract;
		// Properties
		property Codec^ Checksum;
		// Events
		event DataAvailableHandler^ DataAvailable;
	};

	public ref class CodecBase abstract : public Codec, public IDisposable
	{
		// Methods
	public:
		CodecBase()
		{
			try
			{
				this->_hInput = GCHandle::Alloc(this->_inBuffer, GCHandleType::Pinned);
				this->_hOutput = GCHandle::Alloc(this->_outBuffer, GCHandleType::Pinned);
			}
			catch (Exception^ Exception {0}exception1)
			{
				this->CleanUp(false);
			}
		}
	public:
		virtual void Add(array<unsigned char>^ data)
		{
			this->Add(data, 0, data->Length);
		}
	public:
		virtual void Add(array<unsigned char>^ data, int offset, int count) abstract
		{
		}
	protected:
		virtual void CleanUp() abstract
		{
		}
	private:
		void CleanUp(bool isDisposing)
		{
			if (!this->_isDisposed)
			{
				this->CleanUp();
				if (this->_hInput->IsAllocated)
				{
					this->_hInput->Free();
				}
				if (this->_hOutput->IsAllocated)
				{
					this->_hOutput->Free();
				}
				this->_isDisposed = true;
			}
		}
	protected:
		void copyInput(array<unsigned char>^ data, int startIndex, int count)
		{
			Array::Copy(data, startIndex, this->_inBuffer, 0, count);
			this->_ztream->next_in = this->_hInput->AddrOfPinnedObject();
			this->_ztream->total_in = 0;
			this->_ztream->avail_in = ((unsigned int) count);
		}
	public:
		virtual void Dispose()
		{
			this->Finish();
			this->CleanUp(true);
		}
	protected:
		virtual void Finalize() override
		{
			try
			{
				this->CleanUp(false);
			}
			finally
			{
				Object->Finalize();
			}
		}
	public:
		virtual void Finish() abstract
		{
		}
	protected:
		void OnDataAvailable()
		{
			if ((this->_ztream->total_out > 0))
			{
				if ((this->DataAvailable != nullptr))
				{
					this->DataAvailable->Invoke(this->_outBuffer, 0, ((int) this->_ztream->total_out));
				}
				this->resetOutput();
			}
		}
	protected:
		void resetOutput()
		{
			this->_ztream->total_out = 0;
			this->_ztream->avail_out = 16384;
			this->_ztream->next_out = this->_hOutput->AddrOfPinnedObject();
		}
	protected:
		void setChecksum(unsigned int newSum)
		{
			this->_checksum = newSum;
		}
		// Fields
	private:
		unsigned int _checksum = 0;
		GCHandle _hInput;
		GCHandle _hOutput;
		initonly array<unsigned char>^ _inBuffer = gcnew array<unsigned char>(16384);
		initonly array<unsigned char>^ _outBuffer = gcnew array<unsigned char>(16384);
		DataAvailableHandler^ DataAvailable;
	protected:
		bool _isDisposed = false;
		literal int kBufferSize = 16384;
	internal:
		ZStream _ztream;
	};

	public ref class Inflater : CodecBase
	{
		// Methods
	public:
		Inflater()
		{
			try
			{
				int retval = Inflater::inflateInit_(this->_ztream, Info::Version, Marshal::SizeOf(CodecBase->_ztream));
				if ((retval != 0))
				{
					throw gcnew ZLibException(retval, "Could not initialize inflater") ;
				}
				CodecBase->resetOutput();
			}
			catch (Exception^ Exception {2}exception1)
			{
			}
		}
	public:
		virtual void Add(array<unsigned char>^ data, int offset, int count) override
		{
			if ((data == nullptr))
			{
				throw gcnew ArgumentNullException() ;
			}
			if (((offset < 0) || (count < 0)))
			{
				throw gcnew ArgumentOutOfRangeException() ;
			}
			if (((offset + count) > data->Length))
			{
				throw gcnew ArgumentException() ;
			}
			int total = count;
			int inputIndex = offset;
			int err = 0;
			while (((err >= 0) && (inputIndex < total)))
			{
				CodecBase->copyInput(data, inputIndex, Math::Min((total - inputIndex), 16384));
				while ((this->_ztream->avail_in > 0))
				{
					err = Inflater::inflate(this->_ztream, 0);
					CodecBase->OnDataAvailable();
					if ((err != 0))
					{
						break;
					}
				}
				inputIndex = (inputIndex + ((int) this->_ztream->total_in));
			}
			CodecBase->setChecksum(this->_ztream->adler);
		}
	protected:
		virtual void CleanUp() override
		{
			Inflater::inflateEnd(this->_ztream);
		}
	public:
		virtual void Finish() override
		{
			int err;
			do
			{
				err = Inflater::inflate(this->_ztream, 4);
				CodecBase->OnDataAvailable();
			}
			while((err == 0));
			CodecBase->setChecksum(this->_ztream->adler);
			Inflater::inflateReset(this->_ztream);
			CodecBase->resetOutput();
		}
	private:
		static int inflate(ZStream% sz, int flush)
		{
			return ZLibC::inflate(sz,flush);
		}
	private:
		static int inflateEnd(ZStream% sz)
		{
			return ZLibC::inflateEnd(sz);
		}
	private:
		static int inflateInit_(ZStream% sz, String^ vs, int size)
		{
			int len = vs->Length+1;
			char * p = (char*)_alloca(len);
			ManagedStringToBuffer(vs, p, len, 0);
			return ZLibC::inflateInit_(sz, p, size);
		}
	private:
		static int inflateReset(ZStream% sz)
		{
			return ZLibC::inflateReset(sz);
		}
	};
	public ref class Deflater sealed
	{
		// Methods
	public:
		Deflater(CompressLevel level)
		{
			int retval = Deflater::deflateInit_(this->_ztream, ((int) level), Info::Version, Marshal::SizeOf(CodecBase->_ztream));
			if ((retval != 0))
			{
				throw gcnew ZLibException(retval, "Could not initialize deflater") ;
			}
			CodecBase->resetOutput();
		}
	public:
		virtual void Add(array<unsigned char>^ data, int offset, int count) override
		{
			if ((data == nullptr))
			{
				throw gcnew ArgumentNullException() ;
			}
			if (((offset < 0) || (count < 0)))
			{
				throw gcnew ArgumentOutOfRangeException() ;
			}
			if (((offset + count) > data->Length))
			{
				throw gcnew ArgumentException() ;
			}
			int total = count;
			int inputIndex = offset;
			int err = 0;
			while (((err >= 0) && (inputIndex < total)))
			{
				CodecBase->copyInput(data, inputIndex, Math::Min((total - inputIndex), 16384));
				while ((this->_ztream->avail_in > 0))
				{
					err = Deflater::deflate(this->_ztream, 0);
					if ((err < 0))
					{
						throw gcnew ZLibException(err, (this._ztream.msg ?? "deflate failed")) ;
					}
					CodecBase->OnDataAvailable();
					inputIndex = (inputIndex + ((int) this->_ztream->total_in));
				}
			}
			CodecBase->setChecksum(this->_ztream->adler);
		}
	protected:
		virtual void CleanUp() override
		{
			Deflater::deflateEnd(this->_ztream);
		}
	private:
		static [DllImport("zlib123.dll", CallingConvention=CallingConvention::Cdecl)]
		[returnvalue: MarshalAs(::I4)]
		int deflate([MarshalAs(::Struct)]ZStream% sz, [MarshalAs(::I4)]int flush)
		{
		}
	private:
		static [DllImport("zlib123.dll", CallingConvention=CallingConvention::Cdecl)]
		[returnvalue: MarshalAs(::I4)]
		int deflateEnd([MarshalAs(::Struct)]ZStream% sz)
		{
		}
	private:
		static [DllImport("zlib123.dll", CallingConvention=CallingConvention::Cdecl, CharSet=CharSet::Ansi)]
		[returnvalue: MarshalAs(::I4)]
		int deflateInit_([MarshalAs(::Struct)]ZStream% sz, [MarshalAs(::I4)]int level, [MarshalAs(::LPStr)]String^ vs, [MarshalAs(::I4)]int size)
		{
		}
	private:
		static [DllImport("zlib123.dll", CallingConvention=CallingConvention::Cdecl)]
		[returnvalue: MarshalAs(::I4)]
		int deflateReset([MarshalAs(::Struct)]ZStream% sz)
		{
		}
	public:
		virtual void Finish() override
		{
			int err;
			do
			{
				err = Deflater::deflate(this->_ztream, 4);
				CodecBase->OnDataAvailable();
			}
			while((err == 0));
			CodecBase->setChecksum(this->_ztream->adler);
			Deflater::deflateReset(this->_ztream);
			CodecBase->resetOutput();
		}
	};
	public ref class CRC32Checksum sealed
	{
		// Methods
	public:
		CRC32Checksum()
		{
		}
	public:
		CRC32Checksum(unsigned int initialValue) : ChecksumGeneratorBase(initialValue)
		{
		}
	private:
		static [DllImport("zlib123.dll", CallingConvention=CallingConvention::Cdecl)]
		[returnvalue: MarshalAs(::U4)]
		unsigned int crc32([MarshalAs(::U4)]unsigned int crc, [MarshalAs(::I4)]int data, [MarshalAs(::U4)]unsigned int length)
		{
		}
	public:
		virtual void Update(array<unsigned char>^ data, int offset, int count) override
		{
			if (((offset < 0) || (count < 0)))
			{
				throw gcnew ArgumentOutOfRangeException() ;
			}
			if (((offset + count) > data->Length))
			{
				throw gcnew ArgumentException() ;
			}
			GCHandle hData = GCHandle::Alloc(data, GCHandleType::Pinned);
			try
			{
				ChecksumGeneratorBase->_current = CRC32Checksum::crc32(ChecksumGeneratorBase->_current, (hData.AddrOfPinnedObject()->ToInt32() + offset), ((unsigned int) count));
			}
			finally
			{
				hData.Free();
			}
		}
	};
	private ref class CircularBuffer
	{
		// Methods
	public:
		CircularBuffer(int capacity)
		{
			Debug::Assert((capacity > 0));
			this->_buffer = gcnew array<unsigned char>(capacity);
			this->_capacity = capacity;
			this->_head = 0;
			this->_tail = 0;
			this->_size = 0;
		}
	public:
		int Get()
		{
			if ((this->Size == 0))
			{
				return -1;
			}
			int result = this->_buffer[(this->_head++ % this->_capacity)];
			this->_size--;
			return result;
		}
	public:
		int Get(array<unsigned char>^ destination, int offset, int count)
		{
			int trueCount = Math::Min(count, this->Size);
			for (int i = 0 ; (i < trueCount); i++)
			{
				destination[(offset + i)] = this->_buffer[((this->_head + i) % this->_capacity)];
			}
			this->_head = (this->_head + trueCount);
			this->_head = (this->_head % this->_capacity);
			this->_size = (this->_size - trueCount);
			return trueCount;
		}
	public:
		bool Put(unsigned char b)
		{
			if ((this->Size == this->_capacity))
			{
				return false;
			}
			this->_buffer[this->_tail++] = b;
			this->_tail = (this->_tail % this->_capacity);
			this->_size++;
			return true;
		}
	public:
		int Put(array<unsigned char>^ source, int offset, int count)
		{
			Debug::Assert((count > 0));
			int trueCount = Math::Min(count, (this->_capacity - this->Size));
			for (int i = 0 ; (i < trueCount); i++)
			{
				this->_buffer[((this->_tail + i) % this->_capacity)] = source[(offset + i)];
			}
			this->_tail = (this->_tail + trueCount);
			this->_tail = (this->_tail % this->_capacity);
			this->_size = (this->_size + trueCount);
			return trueCount;
		}
		// Fields
	private:
		initonly array<unsigned char>^ _buffer;
	private:
		initonly int _capacity;
	private:
		int _head;
	private:
		int _size;
	private:
		int _tail;
		// Properties
		property CircularBuffer^ Size;
	};
	public interface class ChecksumGenerator
	{
		// Methods
		void Reset() abstract
		{
		}
		void Update(array<unsigned char>^ data) abstract
		{
		}
		void Update(String^ data) abstract
		{
		}
		void Update(String^ data, Encoding^ encoding) abstract
		{
		}
		void Update(array<unsigned char>^ data, int offset, int count) abstract
		{
		}
		// Properties
		property ChecksumGenerator^ Value;
	};
	public ref class ChecksumGeneratorBase abstract : public ChecksumGenerator
	{
		// Methods
	public:
		ChecksumGeneratorBase()
		{
			this->_current = 0;
		}
	public:
		ChecksumGeneratorBase(unsigned int initialValue)
		{
			this->_current = initialValue;
		}
	public:
		virtual void Reset()
		{
			this->_current = 0;
		}
	public:
		virtual void Update(String^ data)
		{
			this->Update(Encoding::UTF8->GetBytes(data));
		}
	public:
		virtual void Update(array<unsigned char>^ data)
		{
			this->Update(data, 0, data->Length);
		}
	public:
		virtual void Update(String^ data, Encoding^ encoding)
		{
			this->Update(encoding->GetBytes(data));
		}
	public:
		virtual void Update(array<unsigned char>^ data, int offset, int count) abstract
		{
		}
		// Fields
	protected:
		unsigned int _current;
		// Properties
		property ChecksumGeneratorBase^ Value;
	};
	public ref class AdlerChecksum sealed
	{
		// Methods
	public:
		AdlerChecksum()
		{
		}
	public:
		AdlerChecksum(unsigned int initialValue) : ChecksumGeneratorBase(initialValue)
		{
		}
	private:
		static [DllImport("zlib123.dll", CallingConvention=CallingConvention::Cdecl)]
		unsigned int adler32(unsigned int adler, int data, unsigned int length)
		{
		}
	public:
		virtual void Update(array<unsigned char>^ data, int offset, int count) override
		{
			if (((offset < 0) || (count < 0)))
			{
				throw gcnew ArgumentOutOfRangeException() ;
			}
			if (((offset + count) > data->Length))
			{
				throw gcnew ArgumentException() ;
			}
			GCHandle hData = GCHandle::Alloc(data, GCHandleType::Pinned);
			try
			{
				ChecksumGeneratorBase->_current = AdlerChecksum::adler32(ChecksumGeneratorBase->_current, (hData.AddrOfPinnedObject()->ToInt32() + offset), ((unsigned int) count));
			}
			finally
			{
				hData.Free();
			}
		}
	};
#endif
}
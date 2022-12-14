//
// RuntimeMethodInfo.cs: The class used to represent methods from the mono runtime.
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InteropServicesCallingConvention = System.Runtime.InteropServices.CallingConvention;
using System.Runtime.Serialization;
#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Security;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace System.Reflection {

	internal struct MonoMethodInfo 
	{
#pragma warning disable 649	
		private Type parent;
		private Type ret;
		internal MethodAttributes attrs;
		internal MethodImplAttributes iattrs;
		private CallingConventions callconv;
#pragma warning restore 649		

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void get_method_info (IntPtr handle, out MonoMethodInfo info);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int get_method_attributes (IntPtr handle);
		
		internal static MonoMethodInfo GetMethodInfo (IntPtr handle)
		{
			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (handle, out info);
			return info;
		}

		internal static Type GetDeclaringType (IntPtr handle)
		{
			return GetMethodInfo (handle).parent;
		}

		internal static Type GetReturnType (IntPtr handle)
		{
			return GetMethodInfo (handle).ret;
		}

		internal static MethodAttributes GetAttributes (IntPtr handle)
		{
			return (MethodAttributes)get_method_attributes (handle);
		}

		internal static CallingConventions GetCallingConvention (IntPtr handle)
		{
			return GetMethodInfo (handle).callconv;
		}

		internal static MethodImplAttributes GetMethodImplementationFlags (IntPtr handle)
		{
			return GetMethodInfo (handle).iattrs;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern ParameterInfo[] get_parameter_info (IntPtr handle, MemberInfo member);

		static internal ParameterInfo[] GetParametersInfo (IntPtr handle, MemberInfo member)
		{
			return get_parameter_info (handle, member);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern MarshalAsAttribute get_retval_marshal (IntPtr handle);

		static internal ParameterInfo GetReturnParameterInfo (RuntimeMethodInfo method)
		{
			return RuntimeParameterInfo.New (GetReturnType (method.mhandle), method, get_retval_marshal (method.mhandle));
		}
	}

	/*
	 * Note: most of this class needs to be duplicated for the contructor, since
	 * the .NET reflection class hierarchy is so broken.
	 */
	[Serializable()]
	[StructLayout (LayoutKind.Sequential)]
	class RuntimeMethodInfo : MethodInfo
	, ISerializable
	{
#pragma warning disable 649
		internal IntPtr mhandle;
		string name;
		Type reftype;
#pragma warning restore 649

		internal BindingFlags BindingFlags {
			get {
				return 0;
			}
		}

		public override Module Module {
			get {
				return GetRuntimeModule ();
			}
		}

		RuntimeType ReflectedTypeInternal {
			get {
				return (RuntimeType) ReflectedType;
			}
		}

        internal override string FormatNameAndSig (bool serialization)
        {
            // Serialization uses ToString to resolve MethodInfo overloads.
            StringBuilder sbName = new StringBuilder(Name);

            // serialization == true: use unambiguous (except for assembly name) type names to distinguish between overloads.
            // serialization == false: use basic format to maintain backward compatibility of MethodInfo.ToString().
            TypeNameFormatFlags format = serialization ? TypeNameFormatFlags.FormatSerialization : TypeNameFormatFlags.FormatBasic;

            if (IsGenericMethod)
                sbName.Append(RuntimeMethodHandle.ConstructInstantiation(this, format));

            sbName.Append("(");
            RuntimeParameterInfo.FormatParameters (sbName, GetParametersNoCopy (), CallingConvention, serialization);
            sbName.Append(")");

            return sbName.ToString();
        }

		public override Delegate CreateDelegate (Type delegateType)
		{
			return Delegate.CreateDelegate (delegateType, this);
		}

		public override Delegate CreateDelegate (Type delegateType, object target)
		{
			return Delegate.CreateDelegate (delegateType, target, this);
		}

        public override String ToString() 
        {
            return ReturnType.FormatTypeName() + " " + FormatNameAndSig(false);
        }

		internal RuntimeModule GetRuntimeModule ()
		{
			return ((RuntimeType)DeclaringType).GetRuntimeModule();
		}

        #region ISerializable Implementation
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            MemberInfoSerializationHolder.GetSerializationInfo(
                info,
                Name,
                ReflectedTypeInternal,
                ToString(),
                SerializationToString(),
                MemberTypes.Method,
                IsGenericMethod & !IsGenericMethodDefinition ? GetGenericArguments() : null);
        }

        internal string SerializationToString()
        {
            return ReturnType.FormatTypeName(true) + " " + FormatNameAndSig(true);
        }
        #endregion

		internal static MethodBase GetMethodFromHandleNoGenericCheck (RuntimeMethodHandle handle)
		{
			return GetMethodFromHandleInternalType_native (handle.Value, IntPtr.Zero, false);
		}

		internal static MethodBase GetMethodFromHandleNoGenericCheck (RuntimeMethodHandle handle, RuntimeTypeHandle reflectedType)
		{
			return GetMethodFromHandleInternalType_native (handle.Value, reflectedType.Value, false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		[PreserveDependency(".ctor(System.Reflection.ExceptionHandlingClause[],System.Reflection.LocalVariableInfo[],System.Byte[],System.Boolean,System.Int32,System.Int32)", "System.Reflection.MethodBody")]
		internal extern static MethodBody GetMethodBodyInternal (IntPtr handle);

		internal static MethodBody GetMethodBody (IntPtr handle)
		{
			return GetMethodBodyInternal (handle);
		}

		internal static MethodBase GetMethodFromHandleInternalType (IntPtr method_handle, IntPtr type_handle) {
			return GetMethodFromHandleInternalType_native (method_handle, type_handle, true);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static MethodBase GetMethodFromHandleInternalType_native (IntPtr method_handle, IntPtr type_handle, bool genericCheck);

		internal RuntimeMethodInfo () {
		}

		internal RuntimeMethodInfo (RuntimeMethodHandle mhandle) {
			this.mhandle = mhandle.Value;
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern string get_name (MethodBase method);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern RuntimeMethodInfo get_base_method (RuntimeMethodInfo method, bool definition);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int get_metadata_token (RuntimeMethodInfo method);

		public override MethodInfo GetBaseDefinition ()
		{
			return get_base_method (this, true);
		}

		// TODO: Remove, needed only for MonoCustomAttribute
		internal MethodInfo GetBaseMethod ()
		{
			return get_base_method (this, false);
		}

		public override ParameterInfo ReturnParameter {
			get {
				return MonoMethodInfo.GetReturnParameterInfo (this);
			}
		}

		public override Type ReturnType {
			get {
				return MonoMethodInfo.GetReturnType (mhandle);
			}
		}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes { 
			get {
				return MonoMethodInfo.GetReturnParameterInfo (this);
			}
		}

		public override int MetadataToken {
			get {
				return get_metadata_token (this);
			}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return MonoMethodInfo.GetMethodImplementationFlags (mhandle);
		}

		public override ParameterInfo[] GetParameters ()
		{
			var src = MonoMethodInfo.GetParametersInfo (mhandle, this);
			if (src.Length == 0)
				return src;

			// Have to clone because GetParametersInfo icall returns cached value
			var dest = new ParameterInfo [src.Length];
			Array.FastCopy (src, 0, dest, 0, src.Length);
			return dest;
		}

		internal override ParameterInfo[] GetParametersInternal ()
		{
			return MonoMethodInfo.GetParametersInfo (mhandle, this);
		}
		
		internal override int GetParametersCount ()
		{
			return MonoMethodInfo.GetParametersInfo (mhandle, this).Length;
		}

		/*
		 * InternalInvoke() receives the parameters correctly converted by the 
		 * binder to match the types of the method signature.
		 * The exc argument is used to capture exceptions thrown by the icall.
		 * Exceptions thrown by the called method propagate normally.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Object InternalInvoke (Object obj, Object[] parameters, out Exception exc);

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override Object Invoke (Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) 
		{
			if (!IsStatic) {
				if (!DeclaringType.IsInstanceOfType (obj)) {
					if (obj == null)
						throw new TargetException ("Non-static method requires a target.");
					else
						throw new TargetException ("Object does not match target type.");
				}
			}

			if (binder == null)
				binder = Type.DefaultBinder;

			/*Avoid allocating an array every time*/
			ParameterInfo[] pinfo = GetParametersInternal ();
			ConvertValues (binder, parameters, pinfo, culture, invokeAttr);

			if (ContainsGenericParameters)
				throw new InvalidOperationException ("Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.");

			Exception exc;
			object o = null;

			if ((invokeAttr & BindingFlags.DoNotWrapExceptions) == 0) {
				try {
					o = InternalInvoke (obj, parameters, out exc);
				} catch (ThreadAbortException) {
					throw;
#if MOBILE
				} catch (MethodAccessException) {
					throw;
#endif
				} catch (OverflowException) {
					throw;
				} catch (Exception e) {
					throw new TargetInvocationException (e);
				}
			}
			else
			{
				o = InternalInvoke (obj, parameters, out exc);
			}

			if (exc != null)
				throw exc;
			return o;
		}

		internal static void ConvertValues (Binder binder, object[] args, ParameterInfo[] pinfo, CultureInfo culture, BindingFlags invokeAttr)
		{
			if (args == null) {
				if (pinfo.Length == 0)
					return;

				throw new TargetParameterCountException ();
			}

			if (pinfo.Length != args.Length)
				throw new TargetParameterCountException ();

			for (int i = 0; i < args.Length; ++i) {
				var arg = args [i];
				var pi = pinfo [i];
				if (arg == Type.Missing) {
					if (pi.DefaultValue == System.DBNull.Value)
						throw new ArgumentException(Environment.GetResourceString("Arg_VarMissNull"),"parameters");

					args [i] = pi.DefaultValue;
					continue;
				}

				var rt = (RuntimeType) pi.ParameterType;
				args [i] = rt.CheckValue (arg, binder, culture, invokeAttr);
			}
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {
				return new RuntimeMethodHandle (mhandle);
			} 
		}
		
		public override MethodAttributes Attributes { 
			get {
				return MonoMethodInfo.GetAttributes (mhandle);
			} 
		}

		public override CallingConventions CallingConvention { 
			get {
				return MonoMethodInfo.GetCallingConvention (mhandle);
			}
		}
		
		public override Type ReflectedType {
			get {
				return reftype;
			}
		}
		public override Type DeclaringType {
			get {
				return MonoMethodInfo.GetDeclaringType (mhandle);
			}
		}
		public override string Name {
			get {
				if (name != null)
					return name;
				return get_name (this);
			}
		}
		
		public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern void GetPInvoke (out PInvokeAttributes flags, out string entryPoint, out string dllName);

		internal object[] GetPseudoCustomAttributes ()
		{
			int count = 0;

			/* MS.NET doesn't report MethodImplAttribute */

			MonoMethodInfo info = MonoMethodInfo.GetMethodInfo (mhandle);
			if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
				count ++;
			if ((info.attrs & MethodAttributes.PinvokeImpl) != 0)
				count ++;
			
			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
				attrs [count ++] = new PreserveSigAttribute ();
			if ((info.attrs & MethodAttributes.PinvokeImpl) != 0) {
				attrs [count ++] = DllImportAttribute.GetCustomAttribute (this);
			}

			return attrs;
		}

		internal CustomAttributeData[] GetPseudoCustomAttributesData ()
		{
			int count = 0;

			/* MS.NET doesn't report MethodImplAttribute */

			MonoMethodInfo info = MonoMethodInfo.GetMethodInfo (mhandle);
			if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
				count++;
			if ((info.attrs & MethodAttributes.PinvokeImpl) != 0)
				count++;

			if (count == 0)
				return null;
			CustomAttributeData[] attrsData = new CustomAttributeData [count];
			count = 0;

			if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
				attrsData [count++] = new CustomAttributeData ((typeof (PreserveSigAttribute)).GetConstructor (Type.EmptyTypes));
			if ((info.attrs & MethodAttributes.PinvokeImpl) != 0)
				attrsData [count++] = GetDllImportAttributeData ();

			return attrsData;
		}

        private CustomAttributeData GetDllImportAttributeData ()
        {
			if ((Attributes & MethodAttributes.PinvokeImpl) == 0)
				return null;

			string entryPoint, dllName = null;
			PInvokeAttributes flags = 0;

			GetPInvoke (out flags, out entryPoint, out dllName);

			CharSet charSet;

			switch (flags & PInvokeAttributes.CharSetMask) {
			case PInvokeAttributes.CharSetNotSpec: 
				charSet = CharSet.None; 
				break;
			case PInvokeAttributes.CharSetAnsi: 
				charSet = CharSet.Ansi; 
				break;
			case PInvokeAttributes.CharSetUnicode: 
				charSet = CharSet.Unicode; 
				break;
			case PInvokeAttributes.CharSetAuto: 
				charSet = CharSet.Auto; 
				break;
			// Invalid: default to CharSet.None
			default: 
				charSet = CharSet.None;
				break;
			}

			InteropServicesCallingConvention callingConvention;

			switch (flags & PInvokeAttributes.CallConvMask) {
			case PInvokeAttributes.CallConvWinapi: 
				callingConvention = InteropServicesCallingConvention.Winapi; 
				break;
			case PInvokeAttributes.CallConvCdecl: 
				callingConvention = InteropServicesCallingConvention.Cdecl; 
				break;
			case PInvokeAttributes.CallConvStdcall: 
				callingConvention = InteropServicesCallingConvention.StdCall; 
				break;
			case PInvokeAttributes.CallConvThiscall: 
				callingConvention = InteropServicesCallingConvention.ThisCall; 
				break;
			case PInvokeAttributes.CallConvFastcall: 
				callingConvention = InteropServicesCallingConvention.FastCall; 
				break;
			// Invalid: default to CallingConvention.Cdecl
			default: 
				callingConvention = InteropServicesCallingConvention.Cdecl;
				break;
			}

			bool exactSpelling = (flags & PInvokeAttributes.NoMangle) != 0;
			bool setLastError = (flags & PInvokeAttributes.SupportsLastError) != 0;
			bool bestFitMapping = (flags & PInvokeAttributes.BestFitMask) == PInvokeAttributes.BestFitEnabled;
			bool throwOnUnmappableChar = (flags & PInvokeAttributes.ThrowOnUnmappableCharMask) == PInvokeAttributes.ThrowOnUnmappableCharEnabled;
			bool preserveSig = (GetMethodImplementationFlags () & MethodImplAttributes.PreserveSig) != 0;

			var ctorArgs = new CustomAttributeTypedArgument [] { 
				new CustomAttributeTypedArgument (typeof (string), dllName),
			};

			var attrType = typeof (DllImportAttribute); 

			var namedArgs = new CustomAttributeNamedArgument [] { 
				new CustomAttributeNamedArgument (attrType.GetField ("EntryPoint"), entryPoint),
				new CustomAttributeNamedArgument (attrType.GetField ("CharSet"), charSet),
				new CustomAttributeNamedArgument (attrType.GetField ("ExactSpelling"), exactSpelling),
				new CustomAttributeNamedArgument (attrType.GetField ("SetLastError"), setLastError),
				new CustomAttributeNamedArgument (attrType.GetField ("PreserveSig"), preserveSig),
				new CustomAttributeNamedArgument (attrType.GetField ("CallingConvention"), callingConvention),
				new CustomAttributeNamedArgument (attrType.GetField ("BestFitMapping"), bestFitMapping),
				new CustomAttributeNamedArgument (attrType.GetField ("ThrowOnUnmappableChar"), throwOnUnmappableChar)
			};

			return new CustomAttributeData (
				attrType.GetConstructor (new[] { typeof (string) }),
				ctorArgs,
				namedArgs);
        }

		public override MethodInfo MakeGenericMethod (Type [] methodInstantiation)
		{
			if (methodInstantiation == null)
				throw new ArgumentNullException ("methodInstantiation");

			if (!IsGenericMethodDefinition)
				throw new InvalidOperationException ("not a generic method definition");

			/*FIXME add GetGenericArgumentsLength() internal vcall to speed this up*/
			if (GetGenericArguments ().Length != methodInstantiation.Length)
				throw new ArgumentException ("Incorrect length");

			bool hasUserType = false;
			foreach (Type type in methodInstantiation) {
				if (type == null)
					throw new ArgumentNullException ();
				if (!(type is RuntimeType))
					hasUserType = true;
			}

			if (hasUserType) {
#if !FULL_AOT_RUNTIME
				if (RuntimeFeature.IsDynamicCodeSupported)
					return new MethodOnTypeBuilderInst (this, methodInstantiation);
#endif
				throw new NotSupportedException ("User types are not supported under full aot");
			}

			MethodInfo ret = MakeGenericMethod_impl (methodInstantiation);
			if (ret == null)
				throw new ArgumentException (String.Format ("The method has {0} generic parameter(s) but {1} generic argument(s) were provided.", GetGenericArguments ().Length, methodInstantiation.Length));
			return ret;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo MakeGenericMethod_impl (Type [] types);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public override extern Type [] GetGenericArguments ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetGenericMethodDefinition_impl ();

		public override MethodInfo GetGenericMethodDefinition ()
		{
			MethodInfo res = GetGenericMethodDefinition_impl ();
			if (res == null)
				throw new InvalidOperationException ();

			return res;
		}

		public override extern bool IsGenericMethodDefinition {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override extern bool IsGenericMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override bool ContainsGenericParameters {
			get {
				if (IsGenericMethod) {
					foreach (Type arg in GetGenericArguments ())
						if (arg.ContainsGenericParameters)
							return true;
				}
				return DeclaringType.ContainsGenericParameters;
			}
		}

		public override MethodBody GetMethodBody () {
			return GetMethodBody (mhandle);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}

#if MOBILE
		static int get_core_clr_security_level ()
		{
			return 1;
		}
#else
		//seclevel { transparent = 0, safe-critical = 1, critical = 2}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int get_core_clr_security_level ();
#endif

		public override bool IsSecurityTransparent {
			get { return get_core_clr_security_level () == 0; }
		}

		public override bool IsSecurityCritical {
			get { return get_core_clr_security_level () > 0; }
		}

		public override bool IsSecuritySafeCritical {
			get { return get_core_clr_security_level () == 1; }
		}

		public sealed override bool HasSameMetadataDefinitionAs (MemberInfo other) => HasSameMetadataDefinitionAsCore<RuntimeMethodInfo> (other);
	}
	
	[Serializable()]
	[StructLayout (LayoutKind.Sequential)]
	class RuntimeConstructorInfo : ConstructorInfo, ISerializable
	{
#pragma warning disable 649
		internal IntPtr mhandle;
		string name;
		Type reftype;
#pragma warning restore 649

		public override Module Module {
			get {
				return GetRuntimeModule ();
			}
		}

		internal RuntimeModule GetRuntimeModule ()
		{
			return RuntimeTypeHandle.GetModule((RuntimeType)DeclaringType);
		}

		internal BindingFlags BindingFlags {
			get {
				return 0;
			}
		}

		RuntimeType ReflectedTypeInternal {
			get {
				return (RuntimeType) ReflectedType;
			}
		}

        #region ISerializable Implementation
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            MemberInfoSerializationHolder.GetSerializationInfo(
                info,
                Name,
                ReflectedTypeInternal,
                ToString(),
                SerializationToString(),
                MemberTypes.Constructor,
                null);
        }

        internal string SerializationToString()
        {
            // We don't need the return type for constructors.
            return FormatNameAndSig(true);
        }

		internal void SerializationInvoke (Object target, SerializationInfo info, StreamingContext context)
		{
			Invoke (target, new object[] { info, context });
		}
       #endregion
		
		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return MonoMethodInfo.GetMethodImplementationFlags (mhandle);
		}

		public override ParameterInfo[] GetParameters ()
		{
			return MonoMethodInfo.GetParametersInfo (mhandle, this);
		}

		internal override ParameterInfo[] GetParametersInternal ()
		{
			return MonoMethodInfo.GetParametersInfo (mhandle, this);
		}		

		internal override int GetParametersCount ()
		{
			var pi = MonoMethodInfo.GetParametersInfo (mhandle, this);
			return pi == null ? 0 : pi.Length;
		}

		/*
		 * InternalInvoke() receives the parameters correctly converted by the binder
		 * to match the types of the method signature.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Object InternalInvoke (Object obj, Object[] parameters, out Exception exc);

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override object Invoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) 
		{
			if (obj == null) {
				if (!IsStatic)
					throw new TargetException ("Instance constructor requires a target");
			} else if (!DeclaringType.IsInstanceOfType (obj)) {
				throw new TargetException ("Constructor does not match target type");				
			}

			return DoInvoke (obj, invokeAttr, binder, parameters, culture);
		}

		object DoInvoke (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) 
		{
			if (binder == null)
				binder = Type.DefaultBinder;

			ParameterInfo[] pinfo = MonoMethodInfo.GetParametersInfo (mhandle, this);

			RuntimeMethodInfo.ConvertValues (binder, parameters, pinfo, culture, invokeAttr);

			if (obj == null && DeclaringType.ContainsGenericParameters)
				throw new MemberAccessException ("Cannot create an instance of " + DeclaringType + " because Type.ContainsGenericParameters is true.");

			if ((invokeAttr & BindingFlags.CreateInstance) != 0 && DeclaringType.IsAbstract) {
				throw new MemberAccessException (String.Format ("Cannot create an instance of {0} because it is an abstract class", DeclaringType));
			}

			return InternalInvoke (obj, parameters, (invokeAttr & BindingFlags.DoNotWrapExceptions) == 0);
		}

		public object InternalInvoke (object obj, object[] parameters, bool wrapExceptions)
		{
			Exception exc;
			object o = null;

			if (wrapExceptions) {
				try {
					o = InternalInvoke (obj, parameters, out exc);
#if MOBILE
				} catch (MethodAccessException) {
					throw;
#endif
				} catch (OverflowException) {
					throw;
				} catch (Exception e) {
					throw new TargetInvocationException (e);
				}
			} else {
				o = InternalInvoke (obj, parameters, out exc);
			}

			if (exc != null)
				throw exc;

			return obj == null ? o : null;
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public override Object Invoke (BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
		{
			return DoInvoke (null, invokeAttr, binder, parameters, culture);
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {
				return new RuntimeMethodHandle (mhandle);
			} 
		}
		
		public override MethodAttributes Attributes { 
			get {
				return MonoMethodInfo.GetAttributes (mhandle);
			} 
		}

		public override CallingConventions CallingConvention { 
			get {
				return MonoMethodInfo.GetCallingConvention (mhandle);
			}
		}
		
		public override bool ContainsGenericParameters {
			get {
				return DeclaringType.ContainsGenericParameters;
			}
		}

		public override Type ReflectedType {
			get {
				return reftype;
			}
		}
		public override Type DeclaringType {
			get {
				return MonoMethodInfo.GetDeclaringType (mhandle);
			}
		}
		public override string Name {
			get {
				if (name != null)
					return name;
				return RuntimeMethodInfo.get_name (this);
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override MethodBody GetMethodBody () {
			return RuntimeMethodInfo.GetMethodBody (mhandle);
		}

		public override string ToString () {
			return "Void " + FormatNameAndSig (false);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}

#if MOBILE
		static int get_core_clr_security_level ()
		{
			return 1;
		}
#else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int get_core_clr_security_level ();
#endif

		public sealed override bool HasSameMetadataDefinitionAs (MemberInfo other) => HasSameMetadataDefinitionAsCore<RuntimeConstructorInfo> (other);

		public override bool IsSecurityTransparent {
			get { return get_core_clr_security_level () == 0; }
		}

		public override bool IsSecurityCritical {
			get { return get_core_clr_security_level () > 0; }
		}

		public override bool IsSecuritySafeCritical {
			get { return get_core_clr_security_level () == 1; }
		}

		public override int MetadataToken {
			get {
				return get_metadata_token (this);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern int get_metadata_token (RuntimeConstructorInfo method);
	}
}

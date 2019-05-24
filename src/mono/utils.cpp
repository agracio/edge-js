#include "edge.h"

v8::Local<v8::String> stringCLR2V8(MonoString* text)
{
    Nan::EscapableHandleScope scope;
    return scope.Escape(Nan::New<v8::String>(
        mono_string_chars(text),
        mono_string_length(text)).ToLocalChecked());
}

MonoString* stringV82CLR(v8::Local<v8::String> text)
{
    Nan::HandleScope scope;
    Nan::Utf8String utf8text(text);
    return mono_string_new(mono_domain_get(), *utf8text);    
}

MonoString* exceptionV82stringCLR(v8::Local<v8::String> exception)
{
    Nan::HandleScope scope;
    if (exception->IsObject())
    {
        v8::Local<v8::String> stack = exception->ToObject(Nan::GetCurrentContext()).ToLocalChecked()->Get(Nan::New<v8::String>("stack").ToLocalChecked());
        if (stack->IsString())
        {
            return stringV82CLR(stack->ToString());
        }
    }

    return stringV82CLR(v8::Handle<v8::String>::Cast(exception));
}

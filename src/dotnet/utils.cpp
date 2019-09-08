#include "edge.h"

v8::Local<v8::String> stringCLR2V8(System::String^ text)
{
    Nan::EscapableHandleScope scope;
    if (text->Length > 0)
    {
        array<unsigned char>^ utf8 = System::Text::Encoding::UTF8->GetBytes(text);
        pin_ptr<unsigned char> ch = &utf8[0];
        return scope.Escape(Nan::New<v8::String>((char*)ch, utf8->Length).ToLocalChecked());
    }
    else
    {
        return scope.Escape(Nan::New<v8::String>("").ToLocalChecked());
    }
}

System::String^ stringV82CLR(v8::Local<v8::String> text)
{
    Nan::HandleScope scope;
    Nan::Utf8String utf8text(text);
    if (*utf8text)
    {
        return gcnew System::String(
            *utf8text, 0, utf8text.length(), System::Text::Encoding::UTF8);
    }
    else
    {
        return System::String::Empty;
    }
}

System::String^ stringV82CLR(Nan::Utf8String& utf8text)
{
    Nan::HandleScope scope;
    if (*utf8text)
    {
        return gcnew System::String(
            *utf8text, 0, utf8text.length(), System::Text::Encoding::UTF8);
    }
    else
    {
        return System::String::Empty;
    }
}
System::String^ exceptionV82stringCLR(v8::Local<v8::Value> exception)
{
    v8::Isolate *isolate = v8::Isolate::GetCurrent();
    v8::Local<v8::Context> context = isolate->GetCurrentContext();
    Nan::HandleScope scope;
    if (exception->IsObject())
    {
        v8::Local<v8::Value> stack = exception->ToObject(context).ToLocalChecked()->Get(Nan::GetCurrentContext(), Nan::New<v8::String>("stack").ToLocalChecked()).ToLocalChecked();
        if (stack->IsString())
        {
            return gcnew System::String(stringV82CLR(stack->ToString(context).ToLocalChecked()));
        }
    }

    return gcnew System::String(stringV82CLR(v8::Local<v8::String>::Cast(exception)));
}

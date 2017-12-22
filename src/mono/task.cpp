#include "edge.h"

TaskStatus System::Threading::Tasks::Task::Status(MonoObject* _this)
{
    MonoProperty* prop = mono_class_get_property_from_name(mono_object_get_class(_this), "Status");
    MonoObject* statusBoxed = mono_property_get_value(prop, _this, NULL, NULL);
    TaskStatus status = *(TaskStatus*)mono_object_unbox(statusBoxed);
    return status;
}

MonoException* System::Threading::Tasks::Task::Exception(MonoObject* _this)
{
    MonoProperty* prop = mono_class_get_property_from_name(mono_object_get_class(_this), "Exception");
    MonoObject* exception = mono_property_get_value(prop, _this, NULL, NULL);
    return (MonoException*)exception;
}

MonoObject* System::Threading::Tasks::Task::Result(MonoObject* _this)
{
    MonoProperty* prop = mono_class_get_property_from_name(mono_object_get_class(_this), "Result");
    MonoObject* result = mono_property_get_value(prop, _this, NULL, NULL);
    return result;
}

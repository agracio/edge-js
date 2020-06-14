// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#include "../pal/pal.h"
#include "../pal/pal_utils.h"
#include "deps_entry.h"
#include "deps_format.h"
#include "../pal/trace.h"


bool deps_entry_t::to_path(const pal::string_t& base, bool look_in_base, pal::string_t* str) const
{
    pal::string_t& candidate = *str;

    candidate.clear();

    // Base directory must be present to obtain full path
    if (base.empty())
    {
        return false;
    }

    // Entry relative path contains '/' separator, sanitize it to use
    // platform separator. Perf: avoid extra copy if it matters.
    pal::string_t pal_relative_path = relative_path;
    if (_X('/') != DIR_SEPARATOR)
    {
        replace_char(&pal_relative_path, _X('/'), DIR_SEPARATOR);
    }

    // Reserve space for the path below
    candidate.reserve(base.length() +
        pal_relative_path.length() + 3);

    candidate.assign(base);
    pal::string_t sub_path = look_in_base ? get_filename(pal_relative_path) : pal_relative_path;
    append_path(&candidate, sub_path.c_str());

    bool exists = pal::file_exists(candidate);
    const pal::char_t* query_type = look_in_base ? _X("Local") : _X("Relative");
    if (!exists)
    {
        trace::verbose(_X("    %s path query did not exist %s"), query_type, candidate.c_str());
        candidate.clear();
    }
    else
    {
        trace::verbose(_X("    %s path query exists %s"), query_type, candidate.c_str());
    }
    return exists;
}

// -----------------------------------------------------------------------------
// Given a "base" directory, yield the local path of this file
//
// Parameters:
//    base - The base directory to look for the relative path of this entry
//    str  - If the method returns true, contains the file path for this deps
//           entry relative to the "base" directory
//
// Returns:
//    If the file exists in the path relative to the "base" directory.
//
bool deps_entry_t::to_dir_path(const pal::string_t& base, pal::string_t* str) const
{
    if (asset_type == asset_types::resources)
    {
        pal::string_t pal_relative_path = relative_path;
        if (_X('/') != DIR_SEPARATOR)
        {
            replace_char(&pal_relative_path, _X('/'), DIR_SEPARATOR);
        }
        pal::string_t ietf_dir = get_directory(pal_relative_path);
        pal::string_t ietf = get_filename(ietf_dir);
        pal::string_t base_ietf_dir = base;
        append_path(&base_ietf_dir, ietf.c_str());
        trace::verbose(_X("Detected a resource asset, will query dir/ietf-tag/resource base: %s asset: %s"), base_ietf_dir.c_str(), asset_name.c_str());
        return to_path(base_ietf_dir, true, str);
    }
    return to_path(base, true, str);
}
// -----------------------------------------------------------------------------
// Given a "base" directory, yield the relative path of this file in the package
// layout.
//
// Parameters:
//    base - The base directory to look for the relative path of this entry
//    str  - If the method returns true, contains the file path for this deps
//           entry relative to the "base" directory
//
// Returns:
//    If the file exists in the path relative to the "base" directory.
//
bool deps_entry_t::to_rel_path(const pal::string_t& base, pal::string_t* str) const
{
    return to_path(base, false, str);
}

// -----------------------------------------------------------------------------
// Given a "base" directory, yield the relative path of this file in the package
// layout.
//
// Parameters:
//    base - The base directory to look for the relative path of this entry
//    str  - If the method returns true, contains the file path for this deps
//           entry relative to the "base" directory
//
// Returns:
//    If the file exists in the path relative to the "base" directory.
//
bool deps_entry_t::to_full_path(const pal::string_t& base, pal::string_t* str) const
{
    str->clear();

    // Base directory must be present to obtain full path
    if (base.empty())
    {
        return false;
    }

    pal::string_t new_base = base;
    append_path(&new_base, pal::to_lower(library_name).c_str());

    append_path(&new_base, library_version.c_str());

    return to_rel_path(new_base, str);
}

// -----------------------------------------------------------------------------
// Given a "base" directory, yield the relative path of this file in the package
// layout if the entry hash matches the hash file in the "base" directory
//
// Parameters:
//    base - The base directory to look for the relative path of this entry and
//           the hash file.
//    str  - If the method returns true, contains the file path for this deps
//           entry relative to the "base" directory
//
// Description:
//    Looks for a file named "{PackageName}.{PackageVersion}.nupkg.{HashAlgorithm}"
//    If the deps entry's {HashAlgorithm}-{HashValue} matches the contents then
//    yields the relative path of this entry in the "base" dir.
//
// Returns:
//    If the file exists in the path relative to the "base" directory and there
//    was hash file match with this deps entry.
//
// See: to_full_path(base, str)
//
bool deps_entry_t::to_hash_matched_path(const pal::string_t& base, pal::string_t* str) const
{
    pal::string_t& candidate = *str;

    candidate.clear();

    // Base directory must be present to perform hash lookup.
    if (base.empty())
    {
        return false;
    }

    // First detect position of hyphen in [Algorithm]-[Hash] in the string.
    size_t pos = library_hash.find(_X("-"));
    if (pos == 0 || pos == pal::string_t::npos)
    {
        trace::verbose(_X("Invalid hash %s value for deps file entry: %s"), library_hash.c_str(), library_name.c_str());
        return false;
    }
    pal::string_t entry_hash = library_hash.substr(pos + 1);

    // If downloaded from NuGet.com, package will be signed, so need to get the original hash from the metadata file    
    const pal::string_t nupkg_metadata_filename = _X(".nupkg.metadata");
    pal::string_t nupkg_metadata_file;
    nupkg_metadata_file.reserve(base.length() + library_name.length() + library_version.length() + nupkg_metadata_filename.length() + 3);
    nupkg_metadata_file.assign(base);
    append_path(&nupkg_metadata_file, pal::to_lower(library_name).c_str());
    append_path(&nupkg_metadata_file, library_version.c_str());
    append_path(&nupkg_metadata_file, nupkg_metadata_filename.c_str());

    if (pal::file_exists(nupkg_metadata_file))
    {
        // Read the contents of the metadata file.
        pal::ifstream_t nupkg_metadata_fs(nupkg_metadata_file);
        if (!nupkg_metadata_fs.good())
        {
            trace::verbose(_X("The hash metadata file is invalid [%s]"), nupkg_metadata_file.c_str());
            return false;
        }
        else
        {
            try
            {
                // Get the "contentHash" value for the pre-signed hash value
                const auto metadata_json = web::json::value::parse(nupkg_metadata_fs);
                const auto& content_hash = metadata_json.at(_X("contentHash")).as_string();
                if(content_hash != entry_hash)            
                {
                    trace::verbose(_X("The metadata hash [%s][%d] did not match entry hash [%s][%d]"),
                        content_hash.c_str(), content_hash.length(), entry_hash.c_str(), entry_hash.length());
                }
                else
                {          
                    // All good, just append the relative dir to base.
                    return to_full_path(base, &candidate);
                }
            }
            catch (const std::exception& je)
            {
                pal::string_t jes;
                (void) pal::utf8_palstring(je.what(), &jes);
                trace::error(_X("A JSON parsing exception occurred in [%s]: %s"), nupkg_metadata_file.c_str(), jes.c_str());
            }
        }
    }

    // Build the nupkg file name. Just reserve approx 8 char_t's for the algorithm name.
    pal::string_t nupkg_filename;
    nupkg_filename.reserve(library_name.length() + 1 + library_version.length() + 16);
    nupkg_filename.append(library_name);
    nupkg_filename.append(_X("."));
    nupkg_filename.append(library_version);
    nupkg_filename.append(_X(".nupkg."));
    nupkg_filename.append(library_hash.substr(0, pos));

    // Build the hash file path str.
    pal::string_t hash_file;
    hash_file.reserve(base.length() + library_name.length() + library_version.length() + nupkg_filename.length() + 3);
    hash_file.assign(base);
    append_path(&hash_file, pal::to_lower(library_name).c_str());
    append_path(&hash_file, library_version.c_str());
    append_path(&hash_file, pal::to_lower(nupkg_filename).c_str());

    // Read the contents of the hash file.
    pal::ifstream_t fstream(hash_file);
    if (!fstream.good())
    {
        trace::verbose(_X("The hash file is invalid [%s]"), hash_file.c_str());
        return false;
    }

    // Obtain the hash from the file.
    std::string hash;
    hash.assign(pal::istreambuf_iterator_t(fstream),
        pal::istreambuf_iterator_t());
    pal::string_t pal_hash;
    if (!pal::utf8_palstring(hash.c_str(), &pal_hash))
    {
        return false;
    }
    
    // Check if contents match deps entry. 
    if (entry_hash != pal_hash)
    {
        trace::verbose(_X("The file hash [%s][%d] did not match entry hash [%s][%d]"),
            pal_hash.c_str(), pal_hash.length(), entry_hash.c_str(), entry_hash.length());
        return false;
    }

    // All good, just append the relative dir to base.
    return to_full_path(base, &candidate);
}

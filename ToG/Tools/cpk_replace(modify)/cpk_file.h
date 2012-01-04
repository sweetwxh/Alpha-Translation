#ifndef _CPK_FILE_H_INCLUDED
#define _CPK_FILE_H_INCLUDED

#include <stdio.h>
#include <stdlib.h>
#include <inttypes.h>
#include <stdint.h>
#include <string.h>

#include "error_stuff.h"

#define BUFF 128

#define DEBUG

struct cpk_file
{
    char *location;
    char *filename;
    uint64_t orig_offset;
    uint64_t offset_diff;
    uint32_t orig_size;
    uint32_t new_size;
    int index;
    int found;
    int copied;
};

char* to_filename(const char *location);
int contains_file_name(const char *query, struct cpk_file *file, int count);
void sort_by_offset(struct cpk_file *file, int count);
uint64_t fix_offset(FILE *outfile, const long offset, const long additional, int index, const char *name, struct cpk_file *file, int file_count);
void fix_file_sizes(FILE *outfile, const long offset, struct cpk_file *file, int i);

#endif /* _CPK_FILE_H_INCLUDED */

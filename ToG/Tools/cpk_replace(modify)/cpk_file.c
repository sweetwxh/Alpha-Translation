#include <stdio.h>
#include <stdlib.h>
#include <inttypes.h>
#include <stdint.h>
#include <string.h>

#include "error_stuff.h"
#include "util.h"
#include "utf_tab.h"
#include "cpk_file.h"

char* to_filename(const char *location) {
    int slash = 0;
    int length = 0;
    for(int i = 0;; i++) {
        length = i;
        if(!location[i]) break;
        if(location[i] == '/' || location[i] == '\\')
            slash = i+1;
    }
    int size = length-slash+1;
    char *fn = (char*) calloc(size, sizeof(char));
    CHECK_ERRNO(!fn, "calloc");
    for(int i = slash, j = 0; j < size; i++, j++) {
        if(!location[i]) break;
        fn[j] = location[i];
    }
    return fn;
}

int contains_file_name(const char *query, struct cpk_file *file, int count)
{
    for(int i = 0; i < count; i++)
        if(!strncmp(file[i].location, query, BUFF))
		{
            return i;
		}
    return -1;
}

void sort_by_offset(struct cpk_file *file, int count)
{
    for(int i = 0; i < count; i++)
    {
        int min = -1;
        for(int j = i; j < count; j++)
            if((min < 0 || file[j].orig_offset < file[min].orig_offset) && file[j].found)
                min = j;
        if(min >= 0)
        {
            struct cpk_file tmp = file[i];
            file[i] = file[min];
            file[min] = tmp;
        }
    }
}

uint64_t fix_offset(FILE *outfile, const long offset, const long additional, int index, const char *name, struct cpk_file *file, int file_count)
{
    long orig_offset = additional + query_utf_8byte(outfile, offset, index, name);
    long off_off = query_utf_offset(outfile, offset, index, name);
    long new_offset = orig_offset;
    for(int i = 0; i < file_count; i++)
    {
        if(orig_offset <= file[i].orig_offset) break;
        else if(file[i].copied) new_offset += file[i].offset_diff;
    }
    unsigned char offset_buf[8];
    write_64_be(new_offset-additional, offset_buf);
    replace_data(outfile, off_off, offset_buf, 8);
    return new_offset;
}

void fix_file_sizes(FILE *outfile, const long offset, struct cpk_file *file, int i)
{
    long file_size_off = query_utf_offset(outfile, offset, file[i].index, "FileSize");
    long extract_size_off = query_utf_offset(outfile, offset, file[i].index, "ExtractSize");
    unsigned char size_buf[4];
    write_32_be(file[i].new_size, size_buf);
    replace_data(outfile, file_size_off, size_buf, 4);
    replace_data(outfile, extract_size_off, size_buf, 4);
    printf("%s changed in size by %ld bytes\n", file[i].filename, (long)(file[i].new_size-file[i].orig_size));
}

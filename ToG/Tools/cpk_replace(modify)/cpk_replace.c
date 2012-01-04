#include <stdio.h>
#include <string.h>

#include "cpk_file.h"
#include "utf_tab.h"
#include "util.h"
#include "error_stuff.h"

void analyze_CPK(FILE *infile, long file_length, struct cpk_file *file, int new_file_count);

int main(int argc, char **argv)
{
    printf("cpk_replace 0.3\n");
    if (argc <= 2)
        fprintf(stderr,"Incorrect program usage\nusage: %s cpk files...\n",argv[0]);
    CHECK_ERRNO(argc <= 2, "argc");

    /* create file list */
    int new_file_count = argc-2;
    struct cpk_file file[new_file_count];
    for (int i = 0; i < new_file_count; i++)
    {
        file[i].location = argv[i+2];
        file[i].filename = to_filename(file[i].location);
        file[i].orig_offset = 0;
        file[i].offset_diff = 0;
        file[i].orig_size = 0;
        file[i].new_size = 0;
        file[i].index = 0;
        file[i].found = 0;
        file[i].copied = 0;
		#ifdef DEBUG
			printf("(Debug)input file location: %s\n(Debug)input file name: %s\n",file[i].location,file[i].filename);
		#endif
    }

    /* open file */
    FILE *infile = fopen(argv[1], "rb");
    CHECK_ERRNO(!infile, "fopen");

    /* get file size */
    CHECK_ERRNO(fseek(infile, 0 , SEEK_END) != 0, "fseek");
    long file_length = ftell(infile);
    CHECK_ERRNO(file_length == -1, "ftell");
    rewind(infile);

    analyze_CPK(infile, file_length, file, new_file_count);

    exit(EXIT_SUCCESS);
}

void analyze_CPK(FILE *infile, long file_length, struct cpk_file *file, int new_file_count)
{
    const long CpkHeader_offset = 0x0;
    char *toc_string_table = NULL;

    /* check header */
    {
        unsigned char buf[4];
        static const char CPK_signature[4] = "CPK "; /* intentionally unterminated */
        get_bytes_seek(CpkHeader_offset, infile, buf, 4);
        CHECK_ERROR (memcmp(buf, CPK_signature, sizeof(CPK_signature)), "CPK signature not found");
    }

    /* check CpkHeader */
    {
        struct utf_query_result result = query_utf_nofail(infile, CpkHeader_offset+0x10, NULL);

        CHECK_ERROR (result.rows != 1, "wrong number of rows in CpkHeader");
    }

    /* get TOC offset */
    long toc_offset = query_utf_8byte(infile, CpkHeader_offset+0x10, 0, "TocOffset");

    /* get content offset */
    long content_offset = query_utf_8byte(infile, CpkHeader_offset+0x10, 0, "ContentOffset");

    /* get file count from CpkHeader */
    long CpkHeader_count = query_utf_4byte(infile, CpkHeader_offset+0x10, 0, "Files");

	#ifdef DEBUG
		printf("(Debug)TocOffset: %d\n(Debug)ContentOffset: %d\n(Debug)Files(Count): %d\n",toc_offset,content_offset,CpkHeader_count);
	#endif

    /* check TOC header */
    {
        unsigned char buf[4];
        static const char TOC_signature[4] = "TOC "; /* intentionally unterminated */
        get_bytes_seek(toc_offset, infile, buf, 4);
        CHECK_ERROR (memcmp(buf, TOC_signature, sizeof(TOC_signature)), "TOC signature not found");
    }

    /* get TOC entry count, string table offset */
    long toc_entries;
    long toc_string_table_offset;
    long toc_string_table_size;
    {
        struct utf_query_result result = query_utf_nofail(infile, toc_offset+0x10, NULL);

        toc_entries = result.rows;
        toc_string_table_offset = toc_offset + 0x10 + 8 + result.string_table_offset;
        toc_string_table_size = result.data_offset - result.string_table_offset;
    }

	#ifdef DEBUG
		printf("(Debug)TocEntries: %d\n(Debug)TocStringTableOffset: %d\n(Debug)TocStringTableSize:%d\n",toc_entries,toc_string_table_offset,toc_string_table_size);
	#endif

    /* check that counts match */
    CHECK_ERROR( toc_entries != CpkHeader_count, "CpkHeader file count and TOC entry count do not match" );

    /* load string table */
    toc_string_table = malloc(toc_string_table_size + 1);
    {
        CHECK_ERRNO(!toc_string_table, "malloc");
        memset(toc_string_table, 0, toc_string_table_size+1);
        get_bytes_seek(toc_string_table_offset, infile,
                (unsigned char *)toc_string_table, toc_string_table_size);
    }

    /* find files in cpk */
    for (int i = 0; i < toc_entries; i++)
    {
        /* get file name */
        const char *file_name = query_utf_string(infile, toc_offset+0x10, i, "FileName", toc_string_table);
        
		const char *dir_name=query_utf_string(infile,toc_offset+0x10,i,"DirName",toc_string_table);
		
		/* to query a file with relative path, remember to free it */
		char* query_file=malloc(strlen(dir_name)+strlen(file_name)+2);

		strcpy(query_file,dir_name);

		char sep[]="/";
		
		strcat(query_file,sep);

		strcat(query_file,file_name);

        int entry_num = contains_file_name(query_file, file, new_file_count);
        if (entry_num >= 0)
        {
			#ifdef DEBUG
				printf("(Debug)FileName length:%d\n(Debug)DirName length:%d\n",strlen(file_name),strlen(dir_name));
				printf("(Debug)query file location:%s\n",query_file);
			#endif

            printf("Found %s/%s in cpk\n",dir_name, file_name);

            /* get file offset */
            file[entry_num].orig_offset = content_offset + query_utf_8byte(infile, toc_offset+0x10, i, "FileOffset");

            /* get file size */
            file[entry_num].orig_size = query_utf_4byte(infile, toc_offset+0x10, i, "FileSize");

            /* save index */
            file[entry_num].index = i;
            file[entry_num].found = 1;
			#ifdef DEBUG
				printf("(Debug)EntryNum:%d\n",entry_num);
			#endif
         }

		/* free the query_file to avoid leak */
		if(query_file)
			free(query_file);
    }

    sort_by_offset(file, new_file_count);
 
    FILE *outfile = fopen("out.cpk", "w+b");

    printf("Starting file copy\n");
    long current = 0;
    for (int i = 0; i < new_file_count; i++)
    {
        if (file[i].found)
        {
            /* copy from infile to start of current new file */
            dump(infile, outfile, current, file[i].orig_offset-current);
            current = file[i].orig_offset;
            
            /* open new file and copy data */
            FILE *newfile = fopen(file[i].location, "rb");
            CHECK_ERRNO(!newfile, "fopen");
            CHECK_ERRNO(fseek(newfile, 0 , SEEK_END) != 0, "fseek");
            file[i].new_size = ftell(newfile);
            CHECK_ERRNO(file_length == -1, "ftell");
            rewind(infile);
            printf("Copying %s(file name:%s)\n", file[i].location,file[i].filename);
            dump(newfile, outfile, 0, file[i].new_size);
            printf("Finished copying %s(file name:%s)\n", file[i].location,file[i].filename);
            fclose(newfile);
            
            /* update information */
            current += file[i].orig_size;
            file[i].copied = 1;
            file[i].offset_diff = file[i].new_size -file[i].orig_size;
        }
    }
     
    /* copy remaining data */
    dump(infile, outfile, current, file_length-current);
    printf("Finished file copy\n");
     
    /* fix toc_offset */
    toc_offset = fix_offset(outfile, CpkHeader_offset+0x10, 0, 0, "TocOffset", file, new_file_count);
    printf("Fixed toc offset\n");

    /* fix file offsets */
    for (int i = 0; i < toc_entries; i++)
        fix_offset(outfile, toc_offset+0x10, content_offset, i, "FileOffset", file, new_file_count);
    printf("Fixed file offsets\n");

    /* fix file sizes */
    for (int i = 0; i < new_file_count; i++)
        fix_file_sizes(outfile, toc_offset+0x10, file, i);

    printf("Finished replacing files and fixing data\n");
    CHECK_ERRNO(fclose(outfile) != 0, "fclose");

    if (toc_string_table)
    {
        free(toc_string_table);
        toc_string_table = NULL;
    }
}

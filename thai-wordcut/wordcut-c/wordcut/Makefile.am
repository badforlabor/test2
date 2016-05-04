INCLUDES= -I$(top_srcdir)

libwordcut_HEADERS=wordcut.h wordcut_dict.h xmalloc.h path.h
libwordcutdir=$(includedir)/wordcut

bin_PROGRAMS=wordcut_mkdict wordcut wordcut_dict_dump wordcut_rule_prepare


wordcut_mkdict_SOURCES=wordcut_mkdict.c
wordcut_SOURCES=wordcut_cli.c
wordcut_LDADD=libwordcut.la
wordcut_CFLAGS = $(AM_CFLAGS)

wordcut_dict_dump_SOURCES=wordcut_dict_dump.c
wordcut_dict_dump_LDADD=libwordcut.la
wordcut_dict_dump_CFLAGS = $(AM_CFLAGS)

wordcut_rule_prepare_SOURCES=wordcut_list.c wordcut_rule_prepare.c \
							 xmalloc.c 	
wordcut_rule_prepare_CFLAGS = $(AM_CFLAGS)

lib_LTLIBRARIES=libwordcut.la
libwordcut_la_SOURCES=wordcut_dict.c wordcut.c xmalloc.c wordcut_list.c

EXTRA_DISTS=path.h.in

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AirPlay.Utils;

namespace AirPlay.DmapTagged
{
    public class DMapTagged
    {
        public class ContentTypeItem
        {
            public string Description { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        private Dictionary<string, ContentTypeItem> _contentTypes = new Dictionary<string, ContentTypeItem>
        {
            {
                "abal",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.browsealbumlistung",
                    Type = "list"
                }
            },
            {
                "abar",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.browseartistlisting",
                    Type = "list"
                }
            },
            {
                "abcp",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.browsecomposerlisting",
                    Type = "list"
                }
            },
            {
                "abgn",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.browsegenrelisting",
                    Type = "list"
                }
            },
            {
                "abpl",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.baseplaylist",
                    Type = "byte"
                }
            },
            {
                "abro",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.databasebrowse",
                    Type = "list"
                }
            },
            {
                "adbs",
                new ContentTypeItem
                {
                    Description = "repsoonse to a /databases/id/items",
                    Name = "daap.databasesongs",
                    Type = "list"
                }
            },
            {
                "aeCR",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.content-rating",
                    Type = "string"
                }
            },
            {
                "aeCS",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.artworkchecksum",
                    Type = "int"
                }
            },
            {
                "aeDL",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.drm-downloader-user-id",
                    Type = "long"
                }
            },
            {
                "aeDP",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.drm-platform-id",
                    Type = "int"
                }
            },
            {
                "aeDR",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.drm-user-id",
                    Type = "long"
                }
            },
            {
                "aeDV",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.drm-versions",
                    Type = "int"
                }
            },
            {
                "aeEN",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.episode-num-str",
                    Type = "int"
                }
            },
            {
                "aeES",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.episode-sort",
                    Type = "int"
                }
            },
            {
                "aeFA",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.drm-family-id",
                    Type = "long"
                }
            },
            {
                "aeGD",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.gapless-enc-dr",
                    Type = "int"
                }
            },
            {
                "aeGE",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "com.apple.itunes.gapless-enc-del",
                    Type = "int"
                }
            },
            {
                "aeGH",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.gapless-heur",
                Type = "int" }
            },
            {
                "aeGR",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.gapless-resy",
                Type = "long" }
            },
            {
                "aeGU",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.gapless-dur",
                Type = "long" }
            },
            {
                "aeGs",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.can-be-genius-seed",
                Type = "byte" }
            },
            {
                "aeHD",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.is-hd-video",
                Type = "byte" }
            },
            {
                "aeHV",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.has-video",
                Type = "byte" }
            },
            {
                "aeK1",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.drm-key1-id",
                Type = "long" }
            },
            {
                "aeK2",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.drm-key2-id",
                Type = "long" }
            },
            {
                "aeMK",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.mediakind",
                Type = "byte" }
            },
            {
                "aeMX",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.movie-info-xml",
                Type = "string" }
            },
            {
                "aeMk",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.extended-media-kind",
                Type = "byte" }
            },
            {
                "aeND",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.non-drm-user-id",
                Type = "long" }
            },
            {
                "aeNV",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.norm-volume",
                Type = "int" }
            },
            {
                "aePC",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.is-podcast",
                Type = "byte" }
            },
            {
                "aePP",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.is-podcast-playlist",
                Type = "byte" }
            },
            {
                "aePS",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.special-playlist",
                Type = "byte" }
            },
            {
                "aeSE",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.store-pers-id",
                Type = "long" }
            },
            {
                "aeSG",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.saved-genius",
                Type = "byte" }
            },
            {
                "aeSN",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.series-name",
                Type = "string" }
            },
            {
                "aeSP",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.smart-playlist",
                Type = "byte" }
            },
            {
                "aeSU",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.season-num",
                Type = "int" }
            },
            {
                "aeXD",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.xid",
                Type = "string" }
            },
            {
                "aels",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.liked-state",
                Type = "byte" }
            },
            {
                "agrp",
                new ContentTypeItem { Description = "",
                Name = "daap.songgrouping",
                Type = "string" }
            },
            {
                "ajal",
                new ContentTypeItem { Description = "",
                Name = "com.apple.itunes.store.album-liked-state",
                Type = "byte" }
            },
            {
                "aply",
                new ContentTypeItem { Description = "response to /databases/id/containers",
                Name = "daap.databaseplaylists",
                Type = "list" }
            },
            {
                "apro",
                new ContentTypeItem { Description = "",
                Name = "daap.protocolversion",
                Type = "version" }
            },
            {
                "apso",
                new ContentTypeItem { Description = "response to /databases/id/containers/id/items",
                Name = "daap.playlistsongs",
                Type = "list" }
            },
            {
                "arif",
                new ContentTypeItem { Description = "",
                Name = "daap.resolveinfo",
                Type = "list" }
            },
            {
                "arsv",
                new ContentTypeItem { Description = "",
                Name = "daap.resolve",
                Type = "list" }
            },
            {
                "asaa",
                new ContentTypeItem { Description = "",
                Name = "daap.songalbumartist",
                Type = "string" }
            },
            {
                "asac",
                new ContentTypeItem { Description = "",
                Name = "daap.songartworkcount",
                Type = "short" }
            },
            {
                "asai",
                new ContentTypeItem { Description = "",
                Name = "daap.songalbumid",
                Type = "long" }
            },
            {
                "asal",
                new ContentTypeItem { Description = "the song ones should be self exp.",
                Name = "daap.songalbum",
                Type = "string" }
            },
            {
                "asar",
                new ContentTypeItem { Description = "",
                Name = "daap.songartist",
                Type = "string" }
            },
            {
                "asas",
                new ContentTypeItem { Description = "",
                Name = "daap.songalbumuserratingstatus",
                Type = "byte" }
            },
            {
                "asbk",
                new ContentTypeItem { Description = "",
                Name = "daap.bookmarkable",
                Type = "byte" }
            },
            {
                "asbr",
                new ContentTypeItem { Description = "",
                Name = "daap.songbitrate",
                Type = "short" }
            },
            {
                "asbt",
                new ContentTypeItem { Description = "",
                Name = "daap.songsbeatsperminute",
                Type = "short" }
            },
            {
                "ascd",
                new ContentTypeItem { Description = "",
                Name = "daap.songcodectype",
                Type = "int" }
            },
            {
                "ascm",
                new ContentTypeItem { Description = "",
                Name = "daap.songcomment",
                Type = "string" }
            },
            {
                "ascn",
                new ContentTypeItem { Description = "",
                Name = "daap.songcontentdescription",
                Type = "string" }
            },
            {
                "asco",
                new ContentTypeItem { Description = "",
                Name = "daap.songcompilation",
                Type = "byte" }
            },
            {
                "ascp",
                new ContentTypeItem { Description = "",
                Name = "daap.songcomposer",
                Type = "string" }
            },
            {
                "ascr",

                new ContentTypeItem { Description = "",
                Name = "daap.songcontentrating",
                Type = "byte" }
            },
            {
                "ascs",
                new ContentTypeItem { Description = "",
                Name = "daap.songcodecsubtype",
                Type = "int" }
            },
            {
                "asct",
                new ContentTypeItem { Description = "",
                Name = "daap.songcategory",
                Type = "string" }
            },
            {
                "asda",
                new ContentTypeItem { Description = "",
                Name = "daap.songdateadded",
                Type = "date" }
            },
            {
                "asdb",
                new ContentTypeItem { Description = "",
                Name = "daap.songdisabled",
                Type = "byte" }
            },
            {
                "asdc",
                new ContentTypeItem { Description = "",
                Name = "daap.songdisccount",
                Type = "short" }
            },
            {
                "asdk",
                new ContentTypeItem { Description = "",
                Name = "daap.songdatakind",
                Type = "byte" }
            },
            {
                "asdm",
                new ContentTypeItem { Description = "",
                Name = "daap.songdatemodified",
                Type = "date" }
            },
            {
                "asdn",
                new ContentTypeItem { Description = "",
                Name = "daap.songdiscnumber",
                Type = "short" }
            },
            {
                "asdt",
                new ContentTypeItem { Description = "",
                Name = "daap.songdescription",
                Type = "string" }
            },
            {
                "ased",
                new ContentTypeItem { Description = "",
                Name = "daap.songextradata",
                Type = "short" }
            },
            {
                "aseq",
                new ContentTypeItem { Description = "",
                Name = "daap.songeqpreset",
                Type = "string" }
            },
            {
                "ases",
                new ContentTypeItem { Description = "",
                Name = "daap.songexcludefromshuffle",
                Type = "byte" }
            },
            {
                "asfm",
                new ContentTypeItem { Description = "",
                Name = "daap.songformat",
                Type = "string" }
            },
            {
                "asgn",
                new ContentTypeItem { Description = "",
                Name = "daap.songgenre",
                Type = "string" }
            },
            {
                "asgp",
                new ContentTypeItem { Description = "",
                Name = "daap.songgapless",
                Type = "byte" }
            },
            {
                "ashp",
                new ContentTypeItem { Description = "",
                Name = "daap.songhasbeenplayed",
                Type = "byte" }
            },
            {
                "askd",
                new ContentTypeItem { Description = "",
                Name = "daap.songlastskipdate",
                Type = "date" }
            },
            {
                "askp",
                new ContentTypeItem { Description = "",
                Name = "daap.songuserskipcount",
                Type = "int" }
            },
            {
                "aslr",
                new ContentTypeItem { Description = "",
                Name = "daap.songalbumuserrating",
                Type = "byte" }
            },
            {
                "asls",
                new ContentTypeItem { Description = "",
                Name = "daap.songlongsize",
                Type = "long" }
            },
            {
                "aspc",
                new ContentTypeItem { Description = "",
                Name = "daap.songuserplaycount",
                Type = "int" }
            },
            {
                "aspl",
                new ContentTypeItem { Description = "",
                Name = "daap.songdateplayed",
                Type = "date" }
            },
            {
                "aspu",
                new ContentTypeItem { Description = "",
                Name = "daap.songpodcasturl",
                Type = "string" }
            },
            {
                "asri",
                new ContentTypeItem { Description = "",
                Name = "daap.songartistid",
                Type = "long" }
            },
            {
                "asrs",
                new ContentTypeItem { Description = "",
                Name = "daap.songuserratingstatus",
                Type = "byte" }
            },
            {
                "asrv",
                new ContentTypeItem { Description = "",
                Name = "daap.songrelativevolume",
                Type = "byte" }
            },
            {
                "assa",
                new ContentTypeItem { Description = "",
                Name = "daap.sortartist",
                Type = "string" }
            },
            {
                "assc",
                new ContentTypeItem { Description = "",
                Name = "daap.sortcomposer",
                Type = "string" }
            },
            {
                "assl",
                new ContentTypeItem { Description = "",
                Name = "daap.sortalbumartist",
                Type = "string" }
            },
            {
                "assn",
                new ContentTypeItem { Description = "",
                Name = "daap.sortname",
                Type = "string" }
            },
            {
                "assp",
                new ContentTypeItem { Description = "(in milliseconds)",
                Name = "daap.songstoptime ",
                Type = "int" }
            },
            {
                "assr",
                new ContentTypeItem { Description = "",
                Name = "daap.songsamplerate",
                Type = "int" }
            },
            {
                "asss",
                new ContentTypeItem { Description = "",
                Name = "daap.sortseriesname",
                Type = "string" }
            },
            {
                "asst",
                new ContentTypeItem { Description = "(in milliseconds)",
                Name = "daap.songstarttime ",
                Type = "int" }
            },
            {
                "assu",
                new ContentTypeItem { Description = "",
                Name = "daap.sortalbum",
                Type = "string" }
            },
            {
                "assz",
                new ContentTypeItem { Description = "",
                Name = "daap.songsize",
                Type = "int" }
            },
            {
                "astc",
                new ContentTypeItem { Description = "",
                Name = "daap.songtrackcount",
                Type = "short" }
            },
            {
                "astm",
                new ContentTypeItem { Description = "(in milliseconds)",
                Name = "daap.songtime",
                Type = "int" }
            },
            {
                "astn",
                new ContentTypeItem { Description = "",
                Name = "daap.songtracknumber",
                Type = "short" }
            },
            {
                "asul",
                new ContentTypeItem { Description = "",
                Name = "daap.songdataurl",
                Type = "string" }
            },
            {
                "asur",
                new ContentTypeItem { Description = "",
                Name = "daap.songuserrating",
                Type = "byte" }
            },
            {
                "asyr",
                new ContentTypeItem { Description = "",
                Name = "daap.songyear",
                Type = "short" }
            },
            {
                "avdb",
                new ContentTypeItem { Description = "response to a /databases",
                Name = "daap.serverdatabases",
                Type = "list" }
            },
            {
                "mbcl",
                new ContentTypeItem { Description = "",
                Name = "dmap.bag",
                Type = "list" }
            },
            {
                "mccr",
                new ContentTypeItem { Description = "the response to the content-codes request",
                Name = "dmap.contentcodesresponse",
                Type = "list" }
            },
            {
                "mcna",
                new ContentTypeItem { Description = "the full name of the code",
                Name = "dmap.contentcodesname",
                Type = "string" }
            },
            {
                "mcnm",
                new ContentTypeItem { Description = "the four letter code",
                Name = "dmap.contentcodesnumber",
                Type = "int" }
            },
            {
                "mcon",
                new ContentTypeItem { Description = "an arbitrary container",
                Name = "dmap.container",
                Type = "list" }
            },
            {
                "mctc",
                new ContentTypeItem { Description = "",
                Name = "dmap.containercount",
                Type = "int" }
            },
            {
                "mcti",
                new ContentTypeItem { Description = "the id of an item in its container",
                Name = "dmap.containeritemid",
                Type = "int" }
            },
            {
                "mcty",
                new ContentTypeItem { Description = "the type of the code (see appendix b for type values)",
                Name = "dmap.contentcodestype",
                Type = "short" }
            },
            {
                "mdcl",
                new ContentTypeItem { Description = "",
                Name = "dmap.dictionary",
                Type = "list" }
            },
            {
                "mdst",
                new ContentTypeItem { Description = "",
                Name = "dmap.downloadstatus",
                Type = "byte" }
            },
            {
                "meia",
                new ContentTypeItem { Description = "",
                Name = "dmap.itemdateadded",
                Type = "date" }
            },
            {
                "meip",
                new ContentTypeItem { Description = "",
                Name = "dmap.itemdateplayed",
                Type = "date" }
            },
            {
                "mext",
                new ContentTypeItem { Description = "",
                Name = "dmap.objectextradata",
                Type = "short" }
            },
            {
                "miid",
                new ContentTypeItem { Description = "an item's id",
                Name = "dmap.itemid",
                Type = "int" }
            },
            {
                "mikd",
                new ContentTypeItem { Description = "the kind of item.  So far, only '2' has been seen, an audio file?",
                Name = "dmap.itemkind",
                Type = "byte" }
            },
            {
                "mimc",
                new ContentTypeItem { Description = "number of items in a container",
                Name = "dmap.itemcount",
                Type = "int" }
            },
            {
                "minm",
                new ContentTypeItem { Description = "an items name",
                Name = "dmap.itemname",
                Type = "string" }
            },
            {
                "mlcl",
                new ContentTypeItem { Description = "a list",
                Name = "dmap.listing",
                Type = "list" }
            },
            {
                "mlid",
                new ContentTypeItem { Description = "the session id for the login session",
                Name = "dmap.sessionid",
                Type = "int" }
            },
            {
                "mlit",
                new ContentTypeItem { Description = "a single item in said list",
                Name = "dmap.listingitem",
                Type = "list" }
            },
            {
                "mlog",
                new ContentTypeItem { Description = "response to a /login",
                Name = "dmap.loginresponse",
                Type = "list" }
            },
            {
                "mpco",
                new ContentTypeItem { Description = "",
                Name = "dmap.parentcontainerid",
                Type = "int" }
            },
            {
                "mper",
                new ContentTypeItem { Description = "a persistent id",
                Name = "dmap.persistentid",
                Type = "long" }
            },
            {
                "mpro",
                new ContentTypeItem { Description = "",
                Name = "dmap.protocolversion",
                Type = "version" }
            },
            {
                "mrco",
                new ContentTypeItem { Description = "number of items returned in a request",
                Name = "dmap.returnedcount",
                Type = "int" }
            },
            {
                "msal",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsuatologout",
                Type = "byte" }
            },
            {
                "msau",
                new ContentTypeItem { Description = "",
                Name = "dmap.authenticationmethod",
                Type = "byte" }
            },
            {
                "msaud",
                new ContentTypeItem { Description = "(should be self explanatory)",
                Name = "dmap.authenticationmethod",
                Type = "byte" }
            },
            {
                "msbr",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsbrowse",
                Type = "byte" }
            },
            {
                "msdc",
                new ContentTypeItem { Description = "",
                Name = "dmap.databasescount",
                Type = "int" }
            },
            {
                "msex",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsextensions",
                Type = "byte" }
            },
            {
                "msix",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsindex",
                Type = "byte" }
            },
            {
                "mslr",
                new ContentTypeItem { Description = "",
                Name = "dmap.loginrequired",
                Type = "byte" }
            },
            {
                "mspi",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportspersistentids",
                Type = "byte" }
            },
            {
                "msqy",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsquery",
                Type = "byte" }
            },
            {
                "msrs",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsresolve",
                Type = "byte" }
            },
            {
                "msrv",
                new ContentTypeItem { Description = "response to a /server-info",
                Name = "dmap.serverinforesponse",
                Type = "list" }
            },
            {
                "mstm",
                new ContentTypeItem { Description = "",
                Name = "dmap.timeoutinterval",
                Type = "int" }
            },
            {
                "msts",
                new ContentTypeItem { Description = "",
                Name = "dmap.statusstring",
                Type = "string" }
            },
            {
                "mstt",
                new ContentTypeItem { Description = "the response status code, these appear to be http status codes, e.g. 200",
                Name = "dmap.status",
                Type = "int" }
            },
            {
                "msup",
                new ContentTypeItem { Description = "",
                Name = "dmap.supportsupdate",
                Type = "byte" }
            },
            {
                "msur",
                new ContentTypeItem { Description = "revision to use for requests",
                Name = "dmap.serverrevision",
                Type = "int" }
            },
            {
                "mtco",
                new ContentTypeItem { Description = "",
                Name = "dmap.specifiedtotalcount number of items in response to a request",
                Type = "int" }
            },
            {
                "mudl",
                new ContentTypeItem { Description = "used in updates?  (document soon)",
                Name = "dmap.deletedidlisting",
                Type = "list" }
            },
            {
                "mupd",
                new ContentTypeItem { Description = "response to a /update",
                Name = "dmap.updateresponse",
                Type = "list" }
            },
            {
                "musr",
                new ContentTypeItem { Description = "",
                Name = "dmap.serverrevision",
                Type = "int" }
            },
            {
                "muty",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "dmap.updatetype",
                    Type = "byte"
                }
            },
            {
                "prsv",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "daap.resolve",
                    Type = "list"
                }
            },
            {
                "caps",
                new ContentTypeItem
                {
                    Description = "",
                    Name = "unknown",
                    Type = "byte"
                }
            }
        };

        public Dictionary<string, object> Decode(byte[] buffer, bool useName = false)
        {
            var output = new Dictionary<string, object>();

            var mem = new MemoryStream(buffer);
            using (var reader = new BinaryReader(mem))
            {
                for (int i = 8; i < buffer.Length;)
                {
                    mem.Position = i;
                    var outputKey = Encoding.ASCII.GetString(reader.ReadBytes(4));

                    var itemLength = reader.ReadUInt32BE();
                    var contentType = _contentTypes[outputKey];

                    object parsedData = null;
                    if (itemLength != 0)
                    {
                        var data = reader.ReadBytes((int)itemLength);

                        var dataMem = new MemoryStream(data);
                        using (var dataReader = new BinaryReader(dataMem))
                        {
                            if (contentType.Type == "byte")
                            {
                                parsedData = dataReader.ReadByte();
                            }
                            else if (contentType.Type == "date")
                            {
                                parsedData = dataReader.ReadInt32BE();
                            }
                            else if (contentType.Type == "short")
                            {
                                parsedData = dataReader.ReadUInt16BE();
                            }
                            else if (contentType.Type == "int")
                            {
                                parsedData = dataReader.ReadUInt32BE();
                            }
                            else if (contentType.Type == "long")
                            {
                                parsedData = dataReader.ReadUInt64BE();
                            }
                            else
                            {
                                parsedData = Encoding.ASCII.GetString(data);
                            }
                        }

                        if (useName)
                        {
                            outputKey = contentType.Name;
                        }

                        if (parsedData != null)
                        {
                            output.Add(outputKey, parsedData);
                        }
                    }

                    i += (int)(8 + itemLength);
                }
            }

            return output;
        }
    }
}

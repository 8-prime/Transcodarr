export interface FilesystemEntry {
  name: string;
  path: string;
  lastModified: string;
}

export interface FilesystemBrowseResponse {
  directories: FilesystemEntry[];
}

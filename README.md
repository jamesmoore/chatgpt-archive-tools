# ChatGPT Archive Tools

<img width="1024" height="183" alt="ChatGPT Export Export" src="https://github.com/user-attachments/assets/3cf24bdf-df3e-48c8-97aa-c10e8d72bff0" />

## About

ChatGPT does not offer a clean, built-in way to save or print conversations for offline or archival use. While some browser extensions exist, they may pose security risks.

This tool enables you to extract and reformat your conversations from the official ChatGPT export ZIP file. You can export your data via [ChatGPT settings - https://chatgpt.com/#settings/DataControls](https://chatgpt.com/#settings/DataControls).

## Use cases
* **Create a readable archive** - Convert your ChatGPT conversations into clean Markdown or HTML files that can be read offline, shared, or backed up.
* **Selective cleanup** - Keep a local copy of the chats you want to delete from the web interface, preserving only the chats you need.
* **Knowledge‑base ingestion** - Import your conversations into a wiki, knowledge base, or documentation system with minimal effort.
* **Account migration** - Export all conversations before switching your ChatGPT account, so you retain a viewable copy of your data in case you need it later.

## Features
* Convert ChatGPT exports into smaller, viewable: 
  * markdown files
  * html (Tailwind or Bootstrap formatted)
  * json files
* Process multiple exports in one sweep, detecting the latest version of each conversation.
* Include uploaded and generated image assets in the markdown (thanks to [GLightbox](https://biati-digital.github.io/glightbox/)).
* Transform web references into markdown footnotes.
* Include code blocks and canvas with syntax highlighting (thanks to [highlight.js](https://highlightjs.org/)).
* Detect and render mathematical notation (thanks to [mathjax](https://www.mathjax.org/))
* Runs on Docker, Windows, Linux and MacOS.
* Fully open source with no usage limits or monetization.

## First steps

Request your export ZIP in ChatGPT settings at [https://chatgpt.com/#settings/DataControls](https://chatgpt.com/#settings/DataControls).

Unzip your ChatGPT export ZIP somewhere - **Important - keep an eye out for any ZIP errors**:
```sh
mkdir ~/chatgpt-export
unzip ~/Downloads/chatgpt_export.zip -d ~/chatgpt-export
```

## ChatGPT Archive Server

The archive server hosts a web UI for browsing your exported conversations. It reads one or more source directories that contain `conversations.json`.

You can provide multiple sources. For bare metal, pass `-s`/`--source` multiple times (for example: `-s dir1 -s dir2`). For Docker, set `SOURCE` to a semicolon-separated list (for example: `SOURCE=/source1;/source2`) and mount each directory.

<details>
  <summary>Archive Server - Bare metal</summary>

  1. Run the server (use `-s`/`--source` to point at your export folder):
  ```sh
  dotnet run --project ./chatgpt.archive.api -- -s ~/chatgpt-export
  ```
  If your filesystem is case-sensitive, use the exact project folder casing.
  2. Open the URL printed in the console (typically `http://localhost:5104`).

</details>

<details>
  <summary>Archive Server - Docker</summary>

  1. Run the container (set the `SOURCE` env var and mount the export folder):
  ```sh
  docker run --rm \
    -p 8080:8080 \
    -e SOURCE=/source \
    -v ~/chatgpt-export:/source:ro \
    ghcr.io/jamesmoore/chatgpt-archive-server:main
  ```
  2. Open `http://localhost:8080`. The container listens on port 8080, so change the host port as needed (for example: `-p 80:8080`). You can also run it behind a reverse proxy like Traefik, Caddy, or Nginx.

</details>

<details>
  <summary>Archive Server - Docker compose</summary>

  1. Create a `docker-compose.yaml` (adapt the `SOURCE` and volume path as needed):
  ```yaml
  services:
    chatgpt-archive-server:
      image: ghcr.io/jamesmoore/chatgpt-archive-server:main
      environment:
        SOURCE: /source
      ports:
        - "8080:8080"
      volumes:
        - ~/chatgpt-export:/source:ro
  ```
  2. Run the container:
  ```sh
  docker compose up
  ```
  3. Open `http://localhost:8080`. The container listens on port 8080, so change the host port as needed. You can also run it behind a reverse proxy like Traefik, Caddy, or Nginx.

</details>

## ChatGPT Exporter

You can export from multiple sources. For bare metal, pass `-s`/`--source` multiple times (for example: `-s dir1 -s dir2`). For Docker, mount each source directory and pass multiple `-s` values (for example: `-s /source1 -s /source2`).

<details>
  <summary>Exporter - Bare metal</summary>

  1. Download the latest binary from the [Releases page](../../releases) and unpack it.
  2. On unix systems you may need to `chmod +x` it.
  3. (Optional) Add it to your `PATH`.
  4. Create a directory for the destination
  ```sh
  mkdir ~/chatgpt-markdown
  ```
  5. Run the tool
  ```sh
  ./chatgpt-exporter -s ~/chatgpt-export -d ~/chatgpt-markdown
  ```
  6. Open `~/chatgpt-markdown` - you’ll see an html and markdown file for each conversation.

</details>

<details>
  <summary>Exporter - Docker</summary>

  1. Create a directory for the destination
  ```sh
  mkdir ~/chatgpt-markdown
  ```
  2. Run the docker command (adapt the `-v ~/chatgpt-export` and `-v ~/chatgpt-markdown` parameters to the directories you have just created)
  ```sh
  docker run --rm \
    -v ~/chatgpt-export:/source:ro \
    -v ~/chatgpt-markdown:/destination \
    ghcr.io/jamesmoore/chatgpt-exporter:latest \
    -s /source \
    -d /destination
  ```
  3. Open `~/chatgpt-markdown` - you’ll see an html and markdown file for each conversation.

</details>

<details>
  <summary>Exporter - Docker compose</summary>

  1. Create a directory for the destination
  ```sh
  mkdir ~/chatgpt-markdown
  ```
  2. Create a `docker-compose.yaml` (adapt the `-v ~/chatgpt-export` and `-v ~/chatgpt-markdown` parameters to the directories you have just created)
  ```yaml
  services:
    chatgptexport:
      command: >
        -s /source
        -d /destination
      # append any other parameters as needed
      image: ghcr.io/jamesmoore/chatgpt-exporter:latest
      volumes:
        - ~/chatgpt-export:/source:ro
        - ~/chatgpt-markdown:/destination
  ```
  3. Run the container:
  ```sh
  docker compose run --rm chatgptexport
  ```
  4. Open `~/chatgpt-markdown` - you’ll see an html and markdown file for each conversation.

</details>

## Complete Usage

|Parameter|Optional?|Usage|Default|
|----|----|----|----|
|`-?`<br>`-h`<br>`--help`||Show help and usage information||
|`--version`||Show version information||
|`-s`<br>`--source`|Required|The source directory/directories containing the unzipped ChatGPT exported files.<br>Must contain at least one conversations.json, in the folder or one of its subfolders.<br>You can specify a parent directory containing multiple exports.<br>You can also specify multiple source directories (eg, -s dir1 -s dir2)||
|`-d`<br>`--destination`|Required|The directory where markdown files and assets are to be created||
|`-e`<br>`--export`||Export mode (`latest` or `complete`) see [below](#export-modes).|`latest`|
|`-j`<br>`--json`||Export to json files (`true` or `false`).|`false`|
|`-m`<br>`--markdown`||Export to markdown files (`true` or `false`).|`true`|
|`--html`||Export to html files (`true` or `false`).|`true`|
|`-hf`<br>`--htmlformat`||Specify format for html exports (`bootstrap` or `tailwind`).|`tailwind`|
|`--showhidden`||Includes hidden content in markdown and html exports.<br>Enabling this will include thinking, web searches, image prompts in the export.|`false`|
|`--validate`||Validate the json against the known and expected schema.|`false`|

## How it works
The source folder must contain a file named conversations.json, which holds all your conversations in JSON format. The conversations.json can be in a subfolder, and you can have multiple subfolders (eg, one for each export if you have created many).

Each conversation is converted into one of more files in the destination folder. Depending on the parameters passed in, json, markdown and html files may be created.
For markdown and html exports, any image assets are also extracted and copied to the destination folder.

### <a name="export-modes"></a>Export modes
There are two export modes - `latest` and `complete`. This is to handle conversations that have multiple branches. In ChatGPT If you click "Try again..." or go back and edit one of your previous messages, this causes the conversation to start a new branch. The old branch is hidden and the conversation continues on the latest branch.

`latest` is the recommended mode, as it will produce an export that contains the latest instance of the conversation. `complete` mode will include all the old hidden branches together in a single document. Because of this the conversation may not be coherent.

### Asset management
Any images generated in ChatGPT that are included in the exports will be copied into the destination directory under a subdirectory named `tool-assets`. The corresponding path in the markdown and html will be updated to a relative path. Similarly images uploaded by the user are copied into `user-assets` and referenced the same way.

## Tips
* Running this on a large export may create many files. It will also overwrite any existing files with the same name. Be sure to choose choose an empty destination directory for the first run.
* Keep a copy of your original export ZIPs. In the future you may want to re‑run the tool to generate updated Markdown or a better format:
  * This program may be improved with new features in the future
  * Someone else may write a better one

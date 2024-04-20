from .data_explorer_service import data_explorer_service


def main():
    data_explorer_service.run(host="0.0.0.0", port=5000, debug=True)


if __name__ == "__main__":
    main()

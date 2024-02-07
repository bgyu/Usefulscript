#include <iostream>
#include <vector>
#include <memory>
#include <cstdlib>

#if defined(_WIN32)
#include <malloc.h>
#elif defined(__linux__)
#include <cstdlib>
#endif

template<typename T>
class AlignedAllocator {
public:
    using value_type = T;
    using pointer = T*;
    using const_pointer = const T*;
    using size_type = std::size_t;
    using difference_type = std::ptrdiff_t;

    template<typename U>
    struct rebind {
        using other = AlignedAllocator<U>;
    };

    AlignedAllocator() noexcept = default;

    template<typename U>
    AlignedAllocator(const AlignedAllocator<U>&) noexcept {}

    pointer allocate(size_type n) {
        void* p;
#if defined(_WIN32)
        p = _aligned_malloc(n * sizeof(T), 64);
        if (!p) throw std::bad_alloc();
#elif defined(__linux__)
        if (posix_memalign(&p, 64, n * sizeof(T)) != 0) {
            throw std::bad_alloc();
        }
#endif
        return static_cast<pointer>(p);
    }

    void deallocate(pointer p, size_type) noexcept {
#if defined(_WIN32)
        _aligned_free(p);
#elif defined(__linux__)
        std::free(p);
#endif
    }
};

int main() {
    // Define a vector of doubles with custom allocator
    std::vector<double, AlignedAllocator<double>> vec;

    // Push some elements
    vec.push_back(3.14);
    vec.push_back(2.718);

    // Access elements
    for (const auto& elem : vec) {
        std::cout << elem << std::endl;
    }

    return 0;
}
